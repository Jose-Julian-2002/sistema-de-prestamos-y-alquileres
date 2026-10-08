using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Propiedades.Queries
{
    public record PropiedadDto(
        int Id,
        string Nombre,
        string? Direccion,
        string CodigoLuz,
        string CodigoAgua,
        string? CodigoGas,
        bool Estado,
        int TipoPropiedadId,
        string TipoPropiedad,
        int TotalContratos);

    public record ListarPropiedadesQuery(
        string? Buscar = null,
        int? TipoPropiedadId = null,
        bool? SoloActivos = null,
        int Pagina = 1,
        int Tamano = 20
    ) : IRequest<PagedResultDto<PropiedadDto>>;

    public class ListarPropiedadesQueryHandler
        : IRequestHandler<ListarPropiedadesQuery, PagedResultDto<PropiedadDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarPropiedadesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<PropiedadDto>> Handle(ListarPropiedadesQuery request, CancellationToken cancellationToken)
        {
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamano = request.Tamano < 1 ? 20 : Math.Min(request.Tamano, 200);

            var query = _context.Propiedades.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Buscar))
            {
                var termino = request.Buscar.Trim();

                // Se busca por nombre, direccion o por cualquiera de los codigos de servicio:
                // es como el cobrador recibe la peticion ("la casa de la calle 5", "codigo 8831").
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(termino.ToLower())
                    || (p.Direccion != null && p.Direccion.Contains(termino))
                    || p.CodigoLuz.Contains(termino)
                    || p.CodigoAgua.Contains(termino)
                    || (p.CodigoGas != null && p.CodigoGas.Contains(termino)));
            }

            if (request.TipoPropiedadId.HasValue)
                query = query.Where(p => p.TipoPropiedadId == request.TipoPropiedadId.Value);

            if (request.SoloActivos.HasValue)
                query = query.Where(p => p.Estado == request.SoloActivos.Value);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(p => new PropiedadDto(
                    p.Id,
                    p.Nombre,
                    p.Direccion,
                    p.CodigoLuz,
                    p.CodigoAgua,
                    p.CodigoGas,
                    p.Estado,
                    p.TipoPropiedadId,
                    p.TipoPropiedad.Nombre,
                    p.Contratos.Count))
                .ToListAsync(cancellationToken);

            return new PagedResultDto<PropiedadDto>(items, total, pagina, tamano);
        }
    }

    public record ObtenerPropiedadQuery(int Id, bool IncluirInactivas = true) : IRequest<PropiedadDto>;

    public class ObtenerPropiedadQueryHandler : IRequestHandler<ObtenerPropiedadQuery, PropiedadDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerPropiedadQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PropiedadDto> Handle(ObtenerPropiedadQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Propiedades.AsNoTracking().AsQueryable();

            if (!request.IncluirInactivas)
                query = query.Where(p => p.Estado);

            var propiedad = await query
                .Where(p => p.Id == request.Id)
                .Select(p => new PropiedadDto(
                    p.Id,
                    p.Nombre,
                    p.Direccion,
                    p.CodigoLuz,
                    p.CodigoAgua,
                    p.CodigoGas,
                    p.Estado,
                    p.TipoPropiedadId,
                    p.TipoPropiedad.Nombre,
                    p.Contratos.Count))
                .FirstOrDefaultAsync(cancellationToken);

            if (propiedad is null)
                throw new InvalidOperationException($"No existe una propiedad con el Id {request.Id}.");

            return propiedad;
        }
    }
}