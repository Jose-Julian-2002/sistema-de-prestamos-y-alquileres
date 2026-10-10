using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Pagos.MetodosPago.Queries
{
    public record MetodoPagoDto(int Id, string Nombre, int TotalPagos);

    // Listado simple pensado para el desplegable del formulario de pago.
    public record ListarMetodosPagoQuery(bool SoloUsados = false) : IRequest<IReadOnlyList<MetodoPagoDto>>;

    public class ListarMetodosPagoQueryHandler
        : IRequestHandler<ListarMetodosPagoQuery, IReadOnlyList<MetodoPagoDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarMetodosPagoQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<MetodoPagoDto>> Handle(ListarMetodosPagoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.MetodosPago.AsNoTracking().AsQueryable();

            if (request.SoloUsados)
                query = query.Where(m => m.Pagos.Count > 0);

            return await query
                .OrderBy(m => m.Nombre)
                .Select(m => new MetodoPagoDto(m.Id, m.Nombre, m.Pagos.Count))
                .ToListAsync(cancellationToken);
        }
    }

    public record ObtenerMetodoPagoQuery(int Id) : IRequest<MetodoPagoDto>;

    public class ObtenerMetodoPagoQueryHandler : IRequestHandler<ObtenerMetodoPagoQuery, MetodoPagoDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerMetodoPagoQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MetodoPagoDto> Handle(ObtenerMetodoPagoQuery request, CancellationToken cancellationToken)
        {
            var metodo = await _context.MetodosPago
                .AsNoTracking()
                .Where(m => m.Id == request.Id)
                .Select(m => new MetodoPagoDto(m.Id, m.Nombre, m.Pagos.Count))
                .FirstOrDefaultAsync(cancellationToken);

            if (metodo is null)
                throw new InvalidOperationException($"No existe un metodo de pago con el Id {request.Id}.");

            return metodo;
        }
    }
}