using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Usuarios.Commands
{
    // Edita los datos de perfil del usuario. El rol NO se toca aqui:
    // cambiar el rol exige el permiso Usuarios.GestionarRoles y va en otro comando.
    public record ActualizarUsuarioCommand(int Id, string Nombre, string Gmail) : IRequest<Unit>;

    public class ActualizarUsuarioCommandHandler : IRequestHandler<ActualizarUsuarioCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarUsuarioCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ActualizarUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            var gmailNormalizado = (request.Gmail ?? string.Empty).ToLower().Trim();

            if (string.IsNullOrWhiteSpace(gmailNormalizado))
                throw new ArgumentException("El Gmail es obligatorio.");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (usuario is null)
                throw new InvalidOperationException($"No existe un usuario con el Id {request.Id}.");

            // El Gmail es unico en la base, hay que excluirse a uno mismo de la comprobacion.
            var gmailEnOtro = await _context.Usuarios
                .AnyAsync(u => u.Gmail == gmailNormalizado && u.Id != request.Id, cancellationToken);

            if (gmailEnOtro)
                throw new InvalidOperationException("Ya existe otro usuario registrado con ese Gmail.");

            usuario.Nombre = request.Nombre.Trim();
            usuario.Gmail = gmailNormalizado;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}