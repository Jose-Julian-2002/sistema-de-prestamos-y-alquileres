using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Roles.Queries
{
    // Permiso asignado a un rol, listo para pintar checkboxes en el frontend.
    public record PermisoAsignadoDto(int Id, string Nombre);

    // Resumen para la tabla de listado de roles.
    public record RolDto(
        int Id,
        string Nombre,
        int CantidadUsuarios,
        int CantidadPermisos,
        bool EsBase,
        bool PuedeGestionarUsuarios);

    public record RolDetalleDto(
        int Id,
        string Nombre,
        bool EsBase,
        bool PuedeGestionarUsuarios,
        int CantidadUsuarios,
        IReadOnlyList<PermisoAsignadoDto> Permisos);

    public record ListarRolesQuery(string? Buscar = null) : IRequest<IReadOnlyList<RolDto>>;

    public class ListarRolesQueryHandler : IRequestHandler<ListarRolesQuery, IReadOnlyList<RolDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarRolesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<RolDto>> Handle(ListarRolesQuery request, CancellationToken cancellationToken)
        {
            // Los roles base son los tres que crea el seed; el frontend los marca
            // como no editables.
            var rolesBase = await ConsultasRol.NombresRolesBaseAsync(_context, cancellationToken);

            var query = _context.Roles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Buscar))
            {
                var termino = request.Buscar.Trim().ToLower();
                query = query.Where(r => r.Nombre.ToLower().Contains(termino));
            }

            var roles = await query
                .OrderBy(r => r.Nombre)
                .Select(r => new
                {
                    r.Id,
                    r.Nombre,
                    PermisoIds = r.Permisos.Select(rp => rp.PermisoId).ToList(),
                    PuedeGestionarUsuarios = r.Permisos
                        .Any(rp => rp.Permiso.Nombre == Permisos.Usuarios.GestionarRoles)
                })
                .ToListAsync(cancellationToken);

            var usuariosPorRol = await _context.Usuarios
                .GroupBy(u => u.RolId)
                .Select(g => new { RolId = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(x => x.RolId, x => x.Cantidad, cancellationToken);

            return roles
                .Select(r => new RolDto(
                    r.Id,
                    r.Nombre,
                    usuariosPorRol.TryGetValue(r.Id, out var cantidad) ? cantidad : 0,
                    r.PermisoIds.Count,
                    rolesBase.Contains(r.Nombre),
                    r.PuedeGestionarUsuarios))
                .ToList();
        }
    }

    public record ObtenerRolQuery(int Id) : IRequest<RolDetalleDto>;

    public class ObtenerRolQueryHandler : IRequestHandler<ObtenerRolQuery, RolDetalleDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerRolQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RolDetalleDto> Handle(ObtenerRolQuery request, CancellationToken cancellationToken)
        {
            var rol = await _context.Roles
                .AsNoTracking()
                .Include(r => r.Permisos)
                .ThenInclude(rp => rp.Permiso)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (rol is null)
                throw new InvalidOperationException($"No existe un rol con el Id {request.Id}.");

            var cantidadUsuarios = await _context.Usuarios
                .CountAsync(u => u.RolId == rol.Id, cancellationToken);

            var rolesBase = await ConsultasRol.NombresRolesBaseAsync(_context, cancellationToken);

            var permisos = rol.Permisos
                .Select(rp => new PermisoAsignadoDto(rp.Permiso.Id, rp.Permiso.Nombre))
                .OrderBy(p => p.Nombre, StringComparer.Ordinal)
                .ToList();

            return new RolDetalleDto(
                rol.Id,
                rol.Nombre,
                rolesBase.Contains(rol.Nombre),
                permisos.Any(p => p.Nombre == Permisos.Usuarios.GestionarRoles),
                cantidadUsuarios,
                permisos);
        }
    }

    // Solo los ids de los permisos ya asignados: los que el frontend debe marcar
    // como seleccionados.
    public record ObtenerPermisosDeRolQuery(int RolId) : IRequest<IReadOnlyList<int>>;

    public class ObtenerPermisosDeRolQueryHandler : IRequestHandler<ObtenerPermisosDeRolQuery, IReadOnlyList<int>>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerPermisosDeRolQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<int>> Handle(ObtenerPermisosDeRolQuery request, CancellationToken cancellationToken)
        {
            return await _context.RolPermisos
                .Where(rp => rp.RolId == request.RolId)
                .Select(rp => rp.PermisoId)
                .ToListAsync(cancellationToken);
        }
    }

    internal static class ConsultasRol
    {
        public static async Task<List<string>> NombresRolesBaseAsync(
            IApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            var nombres = await context.Roles
                .Where(r => r.Nombre == Permisos.NombreRolAdministrador
                         || r.Nombre == Permisos.NombreRolCobrador
                         || r.Nombre == Permisos.NombreRolCliente)
                .Select(r => r.Nombre)
                .ToListAsync(cancellationToken);

            return nombres;
        }
    }
}