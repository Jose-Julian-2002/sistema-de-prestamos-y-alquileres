using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Usuarios.Commands
{
    // Reasigna el rol de un usuario. Es una operacion sensible: por eso viaja en su
    // propio comando, protegido por el permiso Usuarios.GestionarRoles.
    public record CambiarRolUsuarioCommand(int Id, int RolId, int? SolicitanteId) : IRequest<Unit>;

    public class CambiarRolUsuarioCommandHandler : IRequestHandler<CambiarRolUsuarioCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public CambiarRolUsuarioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CambiarRolUsuarioCommand request, CancellationToken cancellationToken)
        {
            ValidacionesUsuario.NoEsElPropio(request.Id, request.SolicitanteId, "cambiar el rol de");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (usuario is null)
                throw new InvalidOperationException($"No existe un usuario con el Id {request.Id}.");

            if (usuario.RolId == request.RolId)
                return Unit.Value;

            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RolId, cancellationToken);

            if (rol is null)
                throw new ArgumentException($"No existe un rol con el Id {request.RolId}.");

            // Si el usuario es administrador activo, no puede quedarse sin otro
            // administrador que lo sustituya.
            if (usuario.Estado)
                await ValidacionesUsuario.EsElUltimoAdministrador(_context, request.Id, cancellationToken);

            usuario.RolId = rol.Id;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}