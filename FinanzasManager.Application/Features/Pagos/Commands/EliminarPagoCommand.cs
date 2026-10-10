using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Pagos.Commands
{
    // Borrado FISICO: quita el recibo y, con el, sus detalles (la FK es Cascade).
    // Los saldos de prestamos y contratos se recalculan solos porque se derivan de
    // la suma de los detalles existentes. Solo deberia usarse para anular un pago
    // registrado por error.
    public record EliminarPagoCommand(int Id) : IRequest<Unit>;

    public class EliminarPagoCommandHandler : IRequestHandler<EliminarPagoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public EliminarPagoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EliminarPagoCommand request, CancellationToken cancellationToken)
        {
            var pago = await _context.Pagos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (pago is null)
                throw new InvalidOperationException($"No existe un pago con el Id {request.Id}.");

            _context.PagoDetalles.RemoveRange(pago.Detalles);
            _context.Pagos.Remove(pago);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}