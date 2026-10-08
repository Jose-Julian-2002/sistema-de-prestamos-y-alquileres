using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Usuarios.Commands
{
    // Alta de usuario desde la administracion. A diferencia del registro publico,
    // aqui si se elige el rol, porque lo hace alguien con permisos para gestionarlos.
    public record CrearUsuarioCommand(string Nombre, string Gmail, string Password, int RolId) : IRequest<int>;

    public class CrearUsuarioCommandHandler : IRequestHandler<CrearUsuarioCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public CrearUsuarioCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<int> Handle(CrearUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            var gmailNormalizado = (request.Gmail ?? string.Empty).ToLower().Trim();

            if (string.IsNullOrWhiteSpace(gmailNormalizado))
                throw new ArgumentException("El Gmail es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ArgumentException("La contrasena debe tener al menos 6 caracteres.");

            var gmailExistente = await _context.Usuarios
                .AnyAsync(u => u.Gmail == gmailNormalizado, cancellationToken);

            if (gmailExistente)
                throw new InvalidOperationException("Ya existe un usuario registrado con ese Gmail.");

            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RolId, cancellationToken);

            if (rol is null)
                throw new ArgumentException($"No existe un rol con el Id {request.RolId}.");

            var passwordHasheada = _passwordHasher.Hash(request.Password);

            var nuevoUsuario = new Usuario(rol.Id, request.Nombre, gmailNormalizado, passwordHasheada);

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoUsuario.Id;
        }
    }
}