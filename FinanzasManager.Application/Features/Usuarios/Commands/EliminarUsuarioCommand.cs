using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Usuarios.Commands
{
    // Borrado logico: el usuario queda con Estado = false y deja de poder entrar.
    // La fila nunca se elimina, porque Contrato y Pago apuntan a Usuario y el
    // historial financiero debe seguir siendo trazable.
    public record EliminarUsuarioCommand(int Id, int? SolicitanteId) : IRequest<Unit>;

    public class EliminarUsuarioCommandHandler : IRequestHandler<EliminarUsuarioCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public EliminarUsuarioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EliminarUsuarioCommand request, CancellationToken cancellationToken)
        {
            ValidacionesUsuario.NoEsElPropio(request.Id, request.SolicitanteId, "eliminar");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (usuario is null)
                throw new InvalidOperationException($"No existe un usuario con el Id {request.Id}.");

            if (!usuario.Estado)
                return Unit.Value;

            await ValidacionesUsuario.EsElUltimoAdministrador(_context, request.Id, cancellationToken);

            usuario.Estado = false;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}