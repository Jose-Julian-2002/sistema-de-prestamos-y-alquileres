using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Usuarios.Commands
{
    public record CambiarPasswordUsuarioCommand(int Id, string PasswordActual, string PasswordNueva) : IRequest<Unit>;

    public class CambiarPasswordUsuarioCommandHandler : IRequestHandler<CambiarPasswordUsuarioCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public CambiarPasswordUsuarioCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Unit> Handle(CambiarPasswordUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.PasswordActual))
                throw new ArgumentException("La contrasena actual es obligatoria.");

            if (string.IsNullOrWhiteSpace(request.PasswordNueva) || request.PasswordNueva.Length < 6)
                throw new ArgumentException("La contrasena nueva debe tener al menos 6 caracteres.");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (usuario is null)
                throw new InvalidOperationException($"No existe un usuario con el Id {request.Id}.");

            // Exigimos la actual para que un administrador no pueda dejar a un
            // usuario fuera de su cuenta sin que este lo sepa.
            if (!_passwordHasher.Verify(request.PasswordActual, usuario.Password))
                throw new UnauthorizedAccessException("La contrasena actual no es correcta.");

            usuario.Password = _passwordHasher.Hash(request.PasswordNueva);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}