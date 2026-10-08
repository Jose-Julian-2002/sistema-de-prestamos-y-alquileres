using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Propiedades.TiposPropiedad.Queries
{
    public record TipoPropiedadDto(int Id, string Nombre, int TotalPropiedades);

    // El listado simple esta pensado para el desplegable del formulario de propiedad:
    // trae el conteo para que el frontend pueda avisar antes de dejar un tipo sin uso.
    public record ListarTiposPropiedadQuery(bool SoloConPropiedades = false) : IRequest<IReadOnlyList<TipoPropiedadDto>>;

    public class ListarTiposPropiedadQueryHandler
        : IRequestHandler<ListarTiposPropiedadQuery, IReadOnlyList<TipoPropiedadDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarTiposPropiedadQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TipoPropiedadDto>> Handle(ListarTiposPropiedadQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TipoPropiedades.AsNoTracking().AsQueryable();

            if (request.SoloConPropiedades)
                query = query.Where(t => t.Propiedades.Count > 0);

            return await query
                .OrderBy(t => t.Nombre)
                .Select(t => new TipoPropiedadDto(t.Id, t.Nombre, t.Propiedades.Count))
                .ToListAsync(cancellationToken);
        }
    }

    public record ObtenerTipoPropiedadQuery(int Id) : IRequest<TipoPropiedadDto>;

    public class ObtenerTipoPropiedadQueryHandler : IRequestHandler<ObtenerTipoPropiedadQuery, TipoPropiedadDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerTipoPropiedadQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TipoPropiedadDto> Handle(ObtenerTipoPropiedadQuery request, CancellationToken cancellationToken)
        {
            var tipo = await _context.TipoPropiedades
                .AsNoTracking()
                .Where(t => t.Id == request.Id)
                .Select(t => new TipoPropiedadDto(t.Id, t.Nombre, t.Propiedades.Count))
                .FirstOrDefaultAsync(cancellationToken);

            if (tipo is null)
                throw new InvalidOperationException($"No existe un tipo de propiedad con el Id {request.Id}.");

            return tipo;
        }
    }
}