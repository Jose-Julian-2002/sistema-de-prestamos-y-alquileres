using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Clientes.Queries
{
    // Proyeccion de cliente. Los conteos de prestamos y contratos llegan del servidor
    // para que la tabla del listado no tenga que pedir prestamos y contratos por separado.
    public record ClienteDto(
        int Id,
        string Nombre,
        string? DocumentoIdentidad,
        string? Telefono,
        DateTime? FechaRegistro,
        bool Estado,
        int TotalPrestamos,
        int TotalContratos);

    public record ListarClientesQuery(
        string? Buscar = null,
        bool? SoloActivos = null,
        int Pagina = 1,
        int Tamano = 20
    ) : IRequest<PagedResultDto<ClienteDto>>;

    public class ListarClientesQueryHandler
        : IRequestHandler<ListarClientesQuery, PagedResultDto<ClienteDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarClientesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<ClienteDto>> Handle(ListarClientesQuery request, CancellationToken cancellationToken)
        {
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamano = request.Tamano < 1 ? 20 : Math.Min(request.Tamano, 200);

            var query = _context.Clientes.AsNoTracking();

            // Un cliente se busca por nombre o por documento: el cobrador suele
            // tener uno de los dos a mano, no los dos.
            if (!string.IsNullOrWhiteSpace(request.Buscar))
            {
                var termino = request.Buscar.Trim();

                query = query.Where(c =>
                    c.Nombre.ToLower().Contains(termino.ToLower())
                    || (c.DocumentoIdentidad != null && c.DocumentoIdentidad.Contains(termino)));
            }

            if (request.SoloActivos.HasValue)
                query = query.Where(c => c.Estado == request.SoloActivos.Value);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(c => c.Nombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(c => new ClienteDto(
                    c.Id,
                    c.Nombre,
                    c.DocumentoIdentidad,
                    c.Telefono,
                    c.FechaRegistro,
                    c.Estado,
                    c.Prestamos.Count,
                    c.Contratos.Count))
                .ToListAsync(cancellationToken);

            return new PagedResultDto<ClienteDto>(items, total, pagina, tamano);
        }
    }

    public record ObtenerClienteQuery(int Id, bool IncluirInactivos = true) : IRequest<ClienteDto>;

    public class ObtenerClienteQueryHandler : IRequestHandler<ObtenerClienteQuery, ClienteDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerClienteQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClienteDto> Handle(ObtenerClienteQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Clientes.AsNoTracking().AsQueryable();

            if (!request.IncluirInactivos)
                query = query.Where(c => c.Estado);

            var cliente = await query
                .Where(c => c.Id == request.Id)
                .Select(c => new ClienteDto(
                    c.Id,
                    c.Nombre,
                    c.DocumentoIdentidad,
                    c.Telefono,
                    c.FechaRegistro,
                    c.Estado,
                    c.Prestamos.Count,
                    c.Contratos.Count))
                .FirstOrDefaultAsync(cancellationToken);

            if (cliente is null)
                throw new InvalidOperationException($"No existe un cliente con el Id {request.Id}.");

            return cliente;
        }
    }
}