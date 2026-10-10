using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Pagos.Commands
{
    // Crea el recibo (Pago) y sus aplicaciones (PagoDetalle). El usuario que registra
    // el pago lo toma el servidor del token, nunca del cuerpo.
    public record CrearPagoCommand(
        DateTime Fecha,
        int MetodoPagoId,
        int UsuarioId,
        IReadOnlyList<DetallePagoRequest> Detalles) : IRequest<int>;

    public class CrearPagoCommandHandler : IRequestHandler<CrearPagoCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CrearPagoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearPagoCommand request, CancellationToken cancellationToken)
        {
            ValidacionesPago.ValidarDetalles(request.Detalles);

            await ValidacionesPago.ValidarMetodoPago(_context, request.MetodoPagoId, cancellationToken);

            var usuarioActivo = await _context.Usuarios
                .AnyAsync(u => u.Id == request.UsuarioId && u.Estado, cancellationToken);

            if (!usuarioActivo)
                throw new InvalidOperationException($"No existe un usuario activo con el Id {request.UsuarioId}.");

            await ValidacionesPago.ValidarPrestamos(_context, request.Detalles, cancellationToken);
            await ValidacionesPago.ValidarContratos(_context, request.Detalles, cancellationToken);

            // El monto del recibo es la suma de sus aplicaciones. Nunca se confia en
            // un montoTotal enviado por el cliente: podria no cuadrar con los detalles.
            var montoTotal = request.Detalles.Sum(d => d.MontoAplicado);

            var nuevoPago = new Pago(request.MetodoPagoId, request.UsuarioId, request.Fecha, montoTotal);

            foreach (var detalle in request.Detalles)
            {
                var fechaPago = detalle.FechaPago ?? request.Fecha;

                nuevoPago.Detalles.Add(new PagoDetalle(
                    0,
                    detalle.PrestamoId,
                    detalle.ContratoId,
                    detalle.MontoAplicado,
                    fechaPago));
            }

            _context.Pagos.Add(nuevoPago);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoPago.Id;
        }
    }
}