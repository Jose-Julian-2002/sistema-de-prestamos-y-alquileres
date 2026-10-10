using System;
using System.Threading;
using System.Threading.Tasks;

using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Commands
{
    // Validaciones compartidas por los commands de prestamos para que el crear y el
    // actualizar apliquen exactamente las mismas reglas de negocio.
    internal static class ValidacionesPrestamo
    {
        public static void Validar(decimal montoCapital, decimal interes, decimal totalCuotas)
        {
            if (montoCapital <= 0)
                throw new ArgumentException("El monto del capital debe ser mayor que cero.");

            if (interes <= 0)
                throw new ArgumentException("El interes mensual debe ser mayor que cero.");

            if (totalCuotas <= 0)
                throw new ArgumentException("La cantidad de cuotas debe ser mayor que cero.");

            // Las cuotas son meses: 10 cuotas = 10 meses. Un prestamo con 2.5 meses
            // no se puede calcular ni repartir en el calendario.
            if (Math.Round(totalCuotas, 2) % 1m != 0m)
                throw new ArgumentException("La cantidad de cuotas debe ser un numero entero de meses.");
        }

        public static async Task ValidarClienteActivo(
            IApplicationDbContext context,
            int clienteId,
            CancellationToken cancellationToken)
        {
            var clienteActivo = await context.Clientes
                .AnyAsync(c => c.Id == clienteId && c.Estado, cancellationToken);

            if (!clienteActivo)
                throw new InvalidOperationException($"No existe un cliente activo con el Id {clienteId}.");
        }
    }
}