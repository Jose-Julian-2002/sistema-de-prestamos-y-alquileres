using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Application.Features.Prestamos;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Pagos.Commands
{
    // Una aplicacion de pago: monto que se abona a un prestamo o a un contrato.
    // La FechaPago del detalle NO se recibe: el servidor la copia de la fecha del
    // recibo (cabecera) para que nada dependa del reloj del cliente.
    public record DetallePagoRequest(
        decimal MontoAplicado,
        int? PrestamoId,
        int? ContratoId);

    // Validaciones compartidas por los commands de pagos.
    public static class ValidacionesPago
    {
        // Cada detalle aplica a UN solo destino. No a los dos, ni a ninguno.
        public static void ValidarDetalles(IReadOnlyList<DetallePagoRequest> detalles)
        {
            if (detalles is null || detalles.Count == 0)
                throw new ArgumentException("El pago debe tener al menos un detalle.");

            foreach (var detalle in detalles)
            {
                if (detalle.MontoAplicado <= 0)
                    throw new ArgumentException("El monto aplicado de cada detalle debe ser mayor que cero.");

                var tienePrestamo = detalle.PrestamoId.HasValue;
                var tieneContrato = detalle.ContratoId.HasValue;

                if (tienePrestamo && tieneContrato)
                    throw new ArgumentException("Un detalle no puede aplicar a un prestamo y a un contrato a la vez.");

                if (!tienePrestamo && !tieneContrato)
                    throw new ArgumentException("Cada detalle debe indicar a que prestamo o contrato aplica.");
            }
        }

        public static async Task ValidarMetodoPago(
            IApplicationDbContext context,
            int metodoPagoId,
            CancellationToken cancellationToken)
        {
            var metodoExiste = await context.MetodosPago
                .AnyAsync(m => m.Id == metodoPagoId, cancellationToken);

            if (!metodoExiste)
                throw new InvalidOperationException($"No existe un metodo de pago con el Id {metodoPagoId}.");
        }

        public static async Task ValidarPrestamos(
            IApplicationDbContext context,
            IEnumerable<DetallePagoRequest> detalles,
            CancellationToken cancellationToken)
        {
            var prestamos = detalles.Where(d => d.PrestamoId.HasValue).ToList();
            if (prestamos.Count == 0)
                return;

            // Se agrupa por prestamo para que dos detalles del mismo prestamo dentro
            // de un pago no puedan quitar mas de lo que queda por pagar.
            foreach (var grupo in prestamos.GroupBy(d => d.PrestamoId!.Value))
            {
                var prestamo = await context.Prestamos
                    .Include(p => p.PagoDetalles)
                    .FirstOrDefaultAsync(p => p.Id == grupo.Key && p.Estado, cancellationToken);

                if (prestamo is null)
                    throw new InvalidOperationException($"No existe un prestamo activo con el Id {grupo.Key}.");

                var calculo = CalculosPrestamo.Calcular(prestamo.MontoCapital, prestamo.Interes, prestamo.TotalCuotas);
                var yaAbonado = prestamo.PagoDetalles.Sum(d => d.MontoAplicado);
                var montoSolicitado = grupo.Sum(d => d.MontoAplicado);

                var saldoPendiente = calculo.TotalPagar - yaAbonado;

                if (montoSolicitado > saldoPendiente)
                    throw new InvalidOperationException(
                        $"El prestamo {grupo.Key} solo tiene {saldoPendiente:N2} Bs por pagar, no alcanza para {montoSolicitado:N2} Bs.");
            }
        }

        public static async Task ValidarContratos(
            IApplicationDbContext context,
            IEnumerable<DetallePagoRequest> detalles,
            CancellationToken cancellationToken)
        {
            var contratos = detalles.Where(d => d.ContratoId.HasValue).ToList();
            if (contratos.Count == 0)
                return;

            foreach (var grupo in contratos.GroupBy(d => d.ContratoId!.Value))
            {
                var contratoActivo = await context.Contratos
                    .AnyAsync(c => c.Id == grupo.Key && c.Estado, cancellationToken);

                if (!contratoActivo)
                    throw new InvalidOperationException($"No existe un contrato activo con el Id {grupo.Key}.");
            }
        }
    }
}