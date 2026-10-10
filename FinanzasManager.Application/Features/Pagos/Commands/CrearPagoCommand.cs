using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Pagos.Commands
{
    // Recibo de cobro MULTIPLE: una sola cabecera que se reparte entre varios
    // destinos (ej. un inquilino paga el alquiler del contrato y una cuota de su
    // prestamo en el mismo recibo). El Id del usuario (cajero) lo toma el servidor
    // del token y la fecha tambien la pone el servidor: aqui no llegan desde el cliente.
    public record CrearPagoCommand(
        int ClienteId,
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
            // 1. Validaciones de reglas de negocio y de existencia.
            ValidacionesPago.ValidarDetalles(request.Detalles);

            var clienteActivo = await _context.Clientes
                .AnyAsync(c => c.Id == request.ClienteId && c.Estado, cancellationToken);

            if (!clienteActivo)
                throw new InvalidOperationException($"No existe un cliente activo con el Id {request.ClienteId}.");

            var usuarioActivo = await _context.Usuarios
                .AnyAsync(u => u.Id == request.UsuarioId && u.Estado, cancellationToken);

            if (!usuarioActivo)
                throw new InvalidOperationException($"No existe un usuario activo con el Id {request.UsuarioId}.");

            await ValidacionesPago.ValidarMetodoPago(_context, request.MetodoPagoId, cancellationToken);
            await ValidacionesPago.ValidarPrestamos(_context, request.Detalles, cancellationToken);
            await ValidacionesPago.ValidarContratos(_context, request.Detalles, cancellationToken);

            // 2. MontoTotal = suma de los MontoAplicado. Nunca se confia en un valor
            //    enviado por el cliente: podria no cuadrar con los detalles.
            var montoTotal = request.Detalles.Sum(d => d.MontoAplicado);

            // 3. La fecha la asigna el servidor. Se usa la hora local porque el
            //    resto del esquema (FechaRegistro de Cliente, Fechas de Prestamo,
            //    etc.) guarda hora local; cambiar a DateTime.UtcNow es un solo lugar.
            var fecha = DateTime.Now;

            // 4. Transaccion explicita: se inserta la cabecera, se recupera el Id
            //    autogenerado y se asocian los detalles. Si algo falla a mitad de
            //    camino, el rollback deja la caja intacta.
            await using var transaccion = await _context.BeginTransactionAsync(cancellationToken);

            var nuevoPago = new Pago(request.MetodoPagoId, request.UsuarioId, request.ClienteId, fecha, montoTotal);

            _context.Pagos.Add(nuevoPago);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var detalle in request.Detalles)
            {
                nuevoPago.Detalles.Add(new PagoDetalle(
                    nuevoPago.Id,
                    detalle.PrestamoId,
                    detalle.ContratoId,
                    detalle.MontoAplicado,
                    fecha));
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);

            return nuevoPago.Id;
        }
    }
}