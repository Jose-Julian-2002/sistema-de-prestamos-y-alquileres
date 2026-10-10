using System;
using System.Threading;
using System.Threading.Tasks;

using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Contratos.Commands
{
    // Validaciones compartidas por los commands de contratos para que el crear y el
    // actualizar apliquen exactamente las mismas reglas de negocio.
    internal static class ValidacionesContrato
    {
        public static void Validar(
            decimal montoAlquiler,
            decimal montoGarantia,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            if (montoAlquiler <= 0)
                throw new ArgumentException("El monto del alquiler debe ser mayor que cero.");

            if (montoGarantia <= 0)
                throw new ArgumentException("El monto de la garantia debe ser mayor que cero.");

            if (fechaInicio > fechaFin)
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin.");
        }

        // El usuario del token se valida por separado: existe porque el token es valido,
        // pero se comprueba igual por si el usuario fue desactivado antes de firmar.
        public static async Task ValidarReferencias(
            IApplicationDbContext context,
            int propiedadId,
            int clienteId,
            int usuarioId,
            CancellationToken cancellationToken)
        {
            var propiedad = await context.Propiedades
                .AnyAsync(p => p.Id == propiedadId && p.Estado, cancellationToken);

            if (!propiedad)
                throw new InvalidOperationException($"No existe una propiedad activa con el Id {propiedadId}.");

            var cliente = await context.Clientes
                .AnyAsync(c => c.Id == clienteId && c.Estado, cancellationToken);

            if (!cliente)
                throw new InvalidOperationException($"No existe un cliente activo con el Id {clienteId}.");

            var usuario = await context.Usuarios
                .AnyAsync(u => u.Id == usuarioId && u.Estado, cancellationToken);

            if (!usuario)
                throw new InvalidOperationException($"No existe un usuario activo con el Id {usuarioId}.");
        }
    }
}