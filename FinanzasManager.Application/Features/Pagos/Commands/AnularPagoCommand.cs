using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Pagos.Commands
{
    // Anulacion LOGICA: el recibo no se borra, se marca como anulado y queda
    // el rastro de quien lo anulo, cuando y por que. El original queda intacto
    // para la auditoria y deja de contar para la cobranza (saldos, morosidad
    // y reportes), porque el motor de cobranza solo mira pagos con Estado = true.
    public record AnularPagoCommand(int PagoId, int AnuladoPorId, string Motivo) : IRequest<Unit>;

    public class AnularPagoCommandHandler : IRequestHandler<AnularPagoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public AnularPagoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(AnularPagoCommand request, CancellationToken cancellationToken)
        {
            // Sin motivo no hay anulacion auditable.
            if (string.IsNullOrWhiteSpace(request.Motivo))
                throw new ArgumentException("Debes indicar el motivo de la anulacion.");

            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p => p.Id == request.PagoId, cancellationToken);

            if (pago is null)
                throw new InvalidOperationException($"No existe un pago con el Id {request.PagoId}.");

            if (!pago.Estado)
                throw new InvalidOperationException($"El pago {request.PagoId} ya estaba anulado.");

            // Las reglas se validan contra el rol guardado en la base, no contra
            // lo que diga el cliente: el token solo aporta el id del usuario.
            await ValidarPermisoDeAnulacion(request, pago.UsuarioId, pago.Fecha, cancellationToken);

            pago.Anular(request.AnuladoPorId, request.Motivo, DateTime.Now);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        // Administrador: cualquier pago valido.
        // Cobrador: solo los que el mismo registro y solo durante el dia actual.
        private async Task ValidarPermisoDeAnulacion(
            AnularPagoCommand request,
            int registradoPorId,
            DateTime fechaPago,
            CancellationToken cancellationToken)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == request.AnuladoPorId, cancellationToken);

            if (usuario is null || !usuario.Estado)
                throw new AccesoDenegadoException("El usuario que intenta anular no existe o esta inactivo.");

            if (usuario.Rol.Nombre == Permisos.NombreRolAdministrador)
                return;

            if (usuario.Rol.Nombre != Permisos.NombreRolCobrador)
                throw new AccesoDenegadoException("Solo un administrador o un cobrador pueden anular pagos.");

            if (registradoPorId != request.AnuladoPorId)
                throw new AccesoDenegadoException("Solo puedes anular los pagos que hayas registrado tu mismo.");

            // DateTime.Now (hora local del servidor), igual que el resto del sistema.
            if (fechaPago.Date != DateTime.Now.Date)
                throw new AccesoDenegadoException(
                    "Solo puedes anular pagos registrados el dia de hoy. "
                    + "Los pagos de dias anteriores solo los puede anular un administrador.");
        }
    }
}
