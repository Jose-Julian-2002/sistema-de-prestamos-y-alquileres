using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Contratos.Commands
{
    // Activa o desactiva el contrato. No borra la fila: los pagos asociados
    // hacen referencia a el, asi que debe conservarse para el historial.
    public record CambiarEstadoContratoCommand(int Id, bool Estado) : IRequest<Unit>;

    public class CambiarEstadoContratoCommandHandler : IRequestHandler<CambiarEstadoContratoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoContratoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CambiarEstadoContratoCommand request, CancellationToken cancellationToken)
        {
            var contrato = await _context.Contratos
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (contrato is null)
                throw new InvalidOperationException($"No existe un contrato con el Id {request.Id}.");

            if (contrato.Estado == request.Estado)
                return Unit.Value;

            contrato.Estado = request.Estado;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}