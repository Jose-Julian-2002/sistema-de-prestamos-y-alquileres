using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Commands
{
    // Borrado logico: el DELETE marca Estado = false. Un prestamo nunca se quita de
    // la base porque la FK Restrict de PagoDetalle no lo permitiria y el historial
    // de abonos debe seguir siendo trazable.
    public record EliminarPrestamoCommand(int Id) : IRequest<Unit>;

    public class EliminarPrestamoCommandHandler : IRequestHandler<EliminarPrestamoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public EliminarPrestamoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EliminarPrestamoCommand request, CancellationToken cancellationToken)
        {
            var prestamo = await _context.Prestamos
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (prestamo is null)
                throw new InvalidOperationException($"No existe un prestamo con el Id {request.Id}.");

            // Idempotente: borrar un prestamo ya desactivado no es un error.
            if (!prestamo.Estado)
                return Unit.Value;

            prestamo.Estado = false;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}