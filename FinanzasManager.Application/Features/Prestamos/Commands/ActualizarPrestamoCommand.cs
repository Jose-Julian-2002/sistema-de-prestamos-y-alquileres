using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Commands
{
    // Edita las condiciones del prestamo. Cambiar capital, interes o cuotas reacalcula
    // la cuota mensual: el frontend debe avisar que los abonos ya registrados pueden
    // dejar de cuadrar con el nuevo plan de pagos.
    public record ActualizarPrestamoCommand(
        int Id,
        int ClienteId,
        decimal MontoCapital,
        decimal Interes,
        decimal TotalCuotas,
        DateTime FechaInicio) : IRequest<Unit>;

    public class ActualizarPrestamoCommandHandler : IRequestHandler<ActualizarPrestamoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarPrestamoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ActualizarPrestamoCommand request, CancellationToken cancellationToken)
        {
            ValidacionesPrestamo.Validar(request.MontoCapital, request.Interes, request.TotalCuotas);

            var prestamo = await _context.Prestamos
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (prestamo is null)
                throw new InvalidOperationException($"No existe un prestamo con el Id {request.Id}.");

            await ValidacionesPrestamo.ValidarClienteActivo(_context, request.ClienteId, cancellationToken);

            prestamo.Actualizar(
                request.ClienteId,
                request.MontoCapital,
                request.Interes,
                request.TotalCuotas,
                request.FechaInicio);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}