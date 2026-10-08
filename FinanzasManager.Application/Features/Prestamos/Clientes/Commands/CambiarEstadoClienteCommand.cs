using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Clientes.Commands
{
    // Activa o desactiva el cliente. No borra la fila: un cliente con prestamos
    // o contratos debe conservar su historial.
    public record CambiarEstadoClienteCommand(int Id, bool Estado) : IRequest<Unit>;

    public class CambiarEstadoClienteCommandHandler : IRequestHandler<CambiarEstadoClienteCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoClienteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CambiarEstadoClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (cliente is null)
                throw new InvalidOperationException($"No existe un cliente con el Id {request.Id}.");

            if (cliente.Estado == request.Estado)
                return Unit.Value;

            cliente.Estado = request.Estado;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}