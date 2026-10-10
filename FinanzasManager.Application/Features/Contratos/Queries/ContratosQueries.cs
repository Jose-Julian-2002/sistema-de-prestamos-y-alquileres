using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Contratos.Queries
{
    public record ContratoDto(
        int Id,
        decimal MontoAlquiler,
        decimal MontoGarantia,
        DateTime DiaPago,
        DateTime FechaInicio,
        DateTime FechaFin,
        bool Estado,
        int PropiedadId,
        string Propiedad,
        int ClienteId,
        string Cliente,
        int UsuarioId,
        string Usuario,
        int TotalPagos);

    public record ListarContratosQuery(
        string? Buscar = null,
        int? ClienteId = null,
        int? PropiedadId = null,
        bool? SoloActivos = null,
        int Pagina = 1,
        int Tamano = 20
    ) : IRequest<PagedResultDto<ContratoDto>>;

    public class ListarContratosQueryHandler
        : IRequestHandler<ListarContratosQuery, PagedResultDto<ContratoDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarContratosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<ContratoDto>> Handle(ListarContratosQuery request, CancellationToken cancellationToken)
        {
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamano = request.Tamano < 1 ? 20 : Math.Min(request.Tamano, 200);

            var query = _context.Contratos.AsNoTracking();

            // Texto libre: se busca en el inquilino, la propiedad o su direccion.
            // Asi el cobrador encuentra el contrato por cualquiera de los datos que recuerde.
            if (!string.IsNullOrWhiteSpace(request.Buscar))
            {
                var termino = request.Buscar.Trim();

                query = query.Where(c =>
                    c.Cliente.Nombre.ToLower().Contains(termino.ToLower())
                    || c.Propiedad.Nombre.ToLower().Contains(termino.ToLower())
                    || (c.Propiedad.Direccion != null && c.Propiedad.Direccion.Contains(termino)));
            }

            if (request.ClienteId.HasValue)
                query = query.Where(c => c.ClienteId == request.ClienteId.Value);

            if (request.PropiedadId.HasValue)
                query = query.Where(c => c.PropiedadId == request.PropiedadId.Value);

            if (request.SoloActivos.HasValue)
                query = query.Where(c => c.Estado == request.SoloActivos.Value);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.FechaInicio)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(c => new ContratoDto(
                    c.Id,
                    c.MontoAlquiler,
                    c.MontoGarantia,
                    c.DiaPago,
                    c.FechaInicio,
                    c.FechaFin,
                    c.Estado,
                    c.PropiedadId,
                    c.Propiedad.Nombre,
                    c.ClienteId,
                    c.Cliente.Nombre,
                    c.UsuarioId,
                    c.Usuario.Nombre,
                    c.PagoDetalles.Count))
                .ToListAsync(cancellationToken);

            return new PagedResultDto<ContratoDto>(items, total, pagina, tamano);
        }
    }

    public record ObtenerContratoQuery(int Id, bool IncluirInactivos = true) : IRequest<ContratoDto>;

    public class ObtenerContratoQueryHandler : IRequestHandler<ObtenerContratoQuery, ContratoDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerContratoQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ContratoDto> Handle(ObtenerContratoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Contratos.AsNoTracking().AsQueryable();

            if (!request.IncluirInactivos)
                query = query.Where(c => c.Estado);

            var contrato = await query
                .Where(c => c.Id == request.Id)
                .Select(c => new ContratoDto(
                    c.Id,
                    c.MontoAlquiler,
                    c.MontoGarantia,
                    c.DiaPago,
                    c.FechaInicio,
                    c.FechaFin,
                    c.Estado,
                    c.PropiedadId,
                    c.Propiedad.Nombre,
                    c.ClienteId,
                    c.Cliente.Nombre,
                    c.UsuarioId,
                    c.Usuario.Nombre,
                    c.PagoDetalles.Count))
                .FirstOrDefaultAsync(cancellationToken);

            if (contrato is null)
                throw new InvalidOperationException($"No existe un contrato con el Id {request.Id}.");

            return contrato;
        }
    }
}