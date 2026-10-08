using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Clientes.Commands
{
    // Borrado logico: el DELETE marca Estado = false en lugar de quitar la fila.
    // Un cliente con prestamos o contratos debe seguir existiendo para que el
    // historial de pagos sea trazable.
    public record EliminarClienteCommand(int Id) : IRequest<Unit>;

    public class EliminarClienteCommandHandler : IRequestHandler<EliminarClienteCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public EliminarClienteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EliminarClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (cliente is null)
                throw new InvalidOperationException($"No existe un cliente con el Id {request.Id}.");

            // Idempotente: borrar un cliente ya desactivado no es un error.
            if (!cliente.Estado)
                return Unit.Value;

            cliente.Estado = false;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}