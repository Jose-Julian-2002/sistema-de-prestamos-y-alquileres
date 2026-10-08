using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Usuarios.Commands
{
    // Activa o desactiva el acceso de un usuario. No borra la fila.
    public record CambiarEstadoUsuarioCommand(int Id, bool Estado, int? SolicitanteId) : IRequest<Unit>;

    public class CambiarEstadoUsuarioCommandHandler : IRequestHandler<CambiarEstadoUsuarioCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoUsuarioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CambiarEstadoUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.Estado)
                ValidacionesUsuario.NoEsElPropio(request.Id, request.SolicitanteId, "desactivar");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (usuario is null)
                throw new InvalidOperationException($"No existe un usuario con el Id {request.Id}.");

            if (usuario.Estado == request.Estado)
                return Unit.Value;

            // No dejar el sistema sin ningun administrador activo.
            if (!request.Estado)
                await ValidacionesUsuario.EsElUltimoAdministrador(_context, request.Id, cancellationToken);

            usuario.Estado = request.Estado;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}