using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Usuarios.Queries
{
    // Proyeccion de usuario para la API. Nunca incluye Password: el hash no sale del servidor.
    public record UsuarioDto(int Id, string Nombre, string Gmail, bool Estado, int RolId, string Rol);

    public record ListarUsuariosQuery(
        string? Buscar = null,
        int? RolId = null,
        bool? SoloActivos = null,
        int Pagina = 1,
        int Tamano = 20
    ) : IRequest<PagedResultDto<UsuarioDto>>;

    public class ListarUsuariosQueryHandler
        : IRequestHandler<ListarUsuariosQuery, PagedResultDto<UsuarioDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarUsuariosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<UsuarioDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken)
        {
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamano = request.Tamano < 1 ? 20 : Math.Min(request.Tamano, 200);

            var query = _context.Usuarios.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Buscar))
            {
                var termino = request.Buscar.Trim().ToLower();
                query = query.Where(u => u.Nombre.ToLower().Contains(termino) || u.Gmail.Contains(termino));
            }

            if (request.RolId.HasValue)
                query = query.Where(u => u.RolId == request.RolId.Value);

            if (request.SoloActivos.HasValue)
                query = query.Where(u => u.Estado == request.SoloActivos.Value);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(u => u.Nombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(u => new UsuarioDto(u.Id, u.Nombre, u.Gmail, u.Estado, u.RolId, u.Rol.Nombre))
                .ToListAsync(cancellationToken);

            return new PagedResultDto<UsuarioDto>(items, total, pagina, tamano);
        }
    }

    public record ObtenerUsuarioQuery(int Id) : IRequest<UsuarioDto>;

    public class ObtenerUsuarioQueryHandler : IRequestHandler<ObtenerUsuarioQuery, UsuarioDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerUsuarioQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UsuarioDto> Handle(ObtenerUsuarioQuery request, CancellationToken cancellationToken)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (usuario is null)
                throw new InvalidOperationException($"No existe un usuario con el Id {request.Id}.");

            return new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Gmail, usuario.Estado, usuario.RolId, usuario.Rol.Nombre);
        }
    }
}