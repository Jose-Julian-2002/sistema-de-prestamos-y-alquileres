using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Pagos.Queries
{
    public record PagoDto(
        int Id,
        DateTime Fecha,
        decimal MontoTotal,
        int MetodoPagoId,
        string MetodoPago,
        int UsuarioId,
        string Usuario,
        int TotalDetalles);

    public record PagoDetalleDto(
        int Id,
        decimal MontoAplicado,
        DateTime FechaPago,
        int? PrestamoId,
        int? ContratoId,
        string? Referencia);

    public record DetallePagoDto(
        PagoDto Pago,
        IReadOnlyList<PagoDetalleDto> Detalles);

    public record ListarPagosQuery(
        int? MetodoPagoId = null,
        int? ClienteId = null,
        int Pagina = 1,
        int Tamano = 20
    ) : IRequest<PagedResultDto<PagoDto>>;

    public class ListarPagosQueryHandler
        : IRequestHandler<ListarPagosQuery, PagedResultDto<PagoDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarPagosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<PagoDto>> Handle(ListarPagosQuery request, CancellationToken cancellationToken)
        {
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamano = request.Tamano < 1 ? 20 : Math.Min(request.Tamano, 200);

            var query = _context.Pagos.AsNoTracking();

            if (request.MetodoPagoId.HasValue)
                query = query.Where(p => p.MetodoPagoId == request.MetodoPagoId.Value);

            // Filtro por cliente: el recibo aparece si alguna de sus aplicaciones
            // toca un prestamo o contrato de ese cliente.
            if (request.ClienteId.HasValue)
            {
                var clienteId = request.ClienteId.Value;
                query = query.Where(p =>
                    p.Detalles.Any(d =>
                        (d.PrestamoId != null && d.Prestamo!.ClienteId == clienteId)
                        || (d.ContratoId != null && d.Contrato!.ClienteId == clienteId)));
            }

            var total = await query.CountAsync(cancellationToken);

            var pagos = await query
                .OrderByDescending(p => p.Fecha)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Include(p => p.MetodoPago)
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                .ToListAsync(cancellationToken);

            var items = pagos.Select(CrearDto).ToList();

            return new PagedResultDto<PagoDto>(items, total, pagina, tamano);
        }

        internal static PagoDto CrearDto(Pago pago)
        {
            return new PagoDto(
                pago.Id,
                pago.Fecha,
                pago.MontoTotal,
                pago.MetodoPagoId,
                pago.MetodoPago.Nombre,
                pago.UsuarioId,
                pago.Usuario.Nombre,
                pago.Detalles.Count);
        }
    }

    public record ObtenerPagoQuery(int Id) : IRequest<DetallePagoDto>;

    public class ObtenerPagoQueryHandler : IRequestHandler<ObtenerPagoQuery, DetallePagoDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerPagoQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DetallePagoDto> Handle(ObtenerPagoQuery request, CancellationToken cancellationToken)
        {
            var pago = await _context.Pagos.AsNoTracking()
                .Include(p => p.MetodoPago)
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Prestamo!)
                        .ThenInclude(pr => pr.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Contrato!)
                        .ThenInclude(c => c.Propiedad)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (pago is null)
                throw new InvalidOperationException($"No existe un pago con el Id {request.Id}.");

            var detalles = pago.Detalles
                .OrderBy(d => d.Id)
                .Select(d =>
                {
                    string? referencia = null;

                    if (d.Prestamo is not null)
                        referencia = $"Prestamo #{d.PrestamoId} - {d.Prestamo.Cliente.Nombre}";

                    if (d.Contrato is not null)
                        referencia = $"Contrato #{d.ContratoId} - {d.Contrato.Propiedad.Nombre}";

                    return new PagoDetalleDto(
                        d.Id,
                        d.MontoAplicado,
                        d.FechaPago,
                        d.PrestamoId,
                        d.ContratoId,
                        referencia);
                })
                .ToList();

            return new DetallePagoDto(ListarPagosQueryHandler.CrearDto(pago), detalles);
        }
    }
}