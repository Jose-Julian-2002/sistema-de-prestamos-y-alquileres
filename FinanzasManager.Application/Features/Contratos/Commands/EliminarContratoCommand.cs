using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Contratos.Commands
{
    // Borrado logico: el DELETE marca Estado = false. Un contrato nunca se quita
    // de la base por que la FK Restrict de PagoDetalle no lo permitiria y el
    // historial de pagos debe seguir siendo trazable.
    public record EliminarContratoCommand(int Id) : IRequest<Unit>;

    public class EliminarContratoCommandHandler : IRequestHandler<EliminarContratoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public EliminarContratoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EliminarContratoCommand request, CancellationToken cancellationToken)
        {
            var contrato = await _context.Contratos
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (contrato is null)
                throw new InvalidOperationException($"No existe un contrato con el Id {request.Id}.");

            // Idempotente: borrar un contrato ya desactivado no es un error.
            if (!contrato.Estado)
                return Unit.Value;

            contrato.Estado = false;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}