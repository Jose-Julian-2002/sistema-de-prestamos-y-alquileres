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

namespace FinanzasManager.Application.Features.Prestamos.Queries
{
    public record PrestamoDto(
        int Id,
        decimal MontoCapital,
        decimal Interes,
        int TotalCuotas,
        DateTime FechaInicio,
        bool Estado,
        int ClienteId,
        string Cliente,
        decimal AbonoCapitalMensual,
        decimal InteresMensual,
        decimal CuotaMensual,
        decimal TotalIntereses,
        decimal TotalPagar,
        decimal TotalAbonado,
        decimal SaldoPendiente);

    public record DetallePrestamoDto(
        PrestamoDto Prestamo,
        IReadOnlyList<CuotaPrestamo> Cuotas);

    public record ListarPrestamosQuery(
        string? Buscar = null,
        int? ClienteId = null,
        bool? SoloActivos = null,
        int Pagina = 1,
        int Tamano = 20
    ) : IRequest<PagedResultDto<PrestamoDto>>;

    public class ListarPrestamosQueryHandler
        : IRequestHandler<ListarPrestamosQuery, PagedResultDto<PrestamoDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarPrestamosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<PrestamoDto>> Handle(ListarPrestamosQuery request, CancellationToken cancellationToken)
        {
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamano = request.Tamano < 1 ? 20 : Math.Min(request.Tamano, 200);

            var query = _context.Prestamos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Buscar))
            {
                var termino = request.Buscar.Trim();

                // Se busca al cliente: el cobrador rara vez recuerda el numero del prestamo.
                query = query.Where(p => p.Cliente.Nombre.ToLower().Contains(termino.ToLower()));
            }

            if (request.ClienteId.HasValue)
                query = query.Where(p => p.ClienteId == request.ClienteId.Value);

            if (request.SoloActivos.HasValue)
                query = query.Where(p => p.Estado == request.SoloActivos.Value);

            var total = await query.CountAsync(cancellationToken);

            var prestamos = await query
                .OrderByDescending(p => p.FechaInicio)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Include(p => p.Cliente)
                .Include(p => p.PagoDetalles)
                .ToListAsync(cancellationToken);

            var items = prestamos.Select(p => CrearDto(p)).ToList();

            return new PagedResultDto<PrestamoDto>(items, total, pagina, tamano);
        }

        internal static PrestamoDto CrearDto(Prestamo p)
        {
            var calculo = CalculosPrestamo.Calcular(p.MontoCapital, p.Interes, p.TotalCuotas);
            var totalAbonado = p.PagoDetalles.Sum(d => d.MontoAplicado);
            var saldoPendiente = calculo.TotalPagar - totalAbonado;

            return new PrestamoDto(
                p.Id,
                p.MontoCapital,
                p.Interes,
                (int)p.TotalCuotas,
                p.FechaInicio,
                p.Estado,
                p.ClienteId,
                p.Cliente.Nombre,
                calculo.AbonoCapitalMensual,
                calculo.InteresMensual,
                calculo.CuotaMensual,
                calculo.TotalIntereses,
                calculo.TotalPagar,
                totalAbonado,
                saldoPendiente);
        }
    }

    public record ObtenerPrestamoQuery(int Id, bool IncluirInactivos = true) : IRequest<DetallePrestamoDto>;

    public class ObtenerPrestamoQueryHandler : IRequestHandler<ObtenerPrestamoQuery, DetallePrestamoDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerPrestamoQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DetallePrestamoDto> Handle(ObtenerPrestamoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Prestamos.AsNoTracking().AsQueryable();

            if (!request.IncluirInactivos)
                query = query.Where(p => p.Estado);

            var prestamo = await query
                .Include(p => p.Cliente)
                .Include(p => p.PagoDetalles)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (prestamo is null)
                throw new InvalidOperationException($"No existe un prestamo con el Id {request.Id}.");

            var cuotas = CalculosPrestamo.GenerarCuotas(
                prestamo.MontoCapital,
                prestamo.Interes,
                prestamo.TotalCuotas);

            return new DetallePrestamoDto(ListarPrestamosQueryHandler.CrearDto(prestamo), cuotas);
        }
    }
}