using System;
using System.Linq;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Autenticacion.Commands
{
    // El rol NO se recibe del cliente: se resuelve en el servidor.
    // Aceptarlo desde el body permitiria registrarse como Administrador.
    public record RegistrarUsuarioCommand(string Nombre, string Gmail, string Password) : IRequest<int>;

    public class RegistrarUsuarioCommandHandler : IRequestHandler<RegistrarUsuarioCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public RegistrarUsuarioCommandHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<int> Handle(RegistrarUsuarioCommand request, CancellationToken cancellationToken)
        {
            // 1. Validaciones basicas de entrada
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            var gmailNormalizado = (request.Gmail ?? string.Empty).ToLower().Trim();

            if (string.IsNullOrWhiteSpace(gmailNormalizado))
                throw new ArgumentException("El Gmail es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ArgumentException("La contrasena debe tener al menos 6 caracteres.");

            // 2. Verificar que el correo no este registrado (el indice unico de la BD tambien lo protege)
            var gmailExistente = await _context.Usuarios
                .AnyAsync(u => u.Gmail == gmailNormalizado, cancellationToken);

            if (gmailExistente)
                throw new InvalidOperationException("Ya existe un usuario registrado con ese Gmail.");

            // 3. El rol por defecto se busca por nombre en la base, nunca se inventa el id.
            var rolPorDefecto = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == Permisos.NombreRolCobrador, cancellationToken);

            if (rolPorDefecto is null)
                throw new InvalidOperationException($"No existe el rol por defecto '{Permisos.NombreRolCobrador}'. Ejecuta el seed de roles.");

            // 4. Hashear la contrasena ANTES de persistir (nunca texto plano)
            var passwordHasheada = _passwordHasher.Hash(request.Password);

            var nuevoUsuario = new Usuario(rolPorDefecto.Id, request.Nombre, gmailNormalizado, passwordHasheada);

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoUsuario.Id;
        }
    }
}