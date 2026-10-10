using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Commands
{
    // Activa o desactiva el prestamo. No borra la fila: los abonos asociados hacen
    // referencia a el, asi que debe conservarse para el historial.
    public record CambiarEstadoPrestamoCommand(int Id, bool Estado) : IRequest<Unit>;

    public class CambiarEstadoPrestamoCommandHandler : IRequestHandler<CambiarEstadoPrestamoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoPrestamoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CambiarEstadoPrestamoCommand request, CancellationToken cancellationToken)
        {
            var prestamo = await _context.Prestamos
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (prestamo is null)
                throw new InvalidOperationException($"No existe un prestamo con el Id {request.Id}.");

            if (prestamo.Estado == request.Estado)
                return Unit.Value;

            prestamo.Estado = request.Estado;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}