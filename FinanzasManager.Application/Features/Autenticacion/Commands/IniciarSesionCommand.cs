using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Autenticacion.Commands
{
    public record IniciarSesionCommand(string Gmail, string Password) : IRequest<AuthResponseDto>;

    // Id incluido para que el frontend pueda comparar con PagoDto.UsuarioId y
    // decidir si muestra "Anular pago". El backend revalida siempre.
    public record AuthResponseDto(int Id, string Token, string Nombre, string Rol, IReadOnlyList<string> Permisos);

    public class IniciarSesionCommandHandler : IRequestHandler<IniciarSesionCommand, AuthResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public IniciarSesionCommandHandler(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> Handle(IniciarSesionCommand request, CancellationToken cancellationToken)
        {
            var gmailNormalizado = (request.Gmail ?? string.Empty).ToLower().Trim();

            // 1. Buscar al usuario activo y traerse su rol con los permisos en la misma consulta,
            //    para no disparar una segunda vuelta a la base al montar el token.
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .ThenInclude(r => r.Permisos)
                .ThenInclude(rp => rp.Permiso)
                .FirstOrDefaultAsync(u => u.Gmail == gmailNormalizado && u.Estado, cancellationToken);

            // 2. Verificar la contrasena contra el hash BCrypt almacenado.
            //    Si el hash esta malformado, Verify devuelve false sin lanzar excepcion.
            if (usuario is null || !_passwordHasher.Verify(request.Password ?? string.Empty, usuario.Password))
            {
                throw new UnauthorizedAccessException("Credenciales incorrectas o usuario inactivo.");
            }

            var permisos = usuario.Rol.Permisos
                .Select(rp => rp.Permiso.Nombre)
                .OrderBy(nombre => nombre)
                .ToList();

            // 3. Generar el token con los claims del usuario y sus permisos.
            var token = _jwtTokenGenerator.GenerarToken(usuario.Id, usuario.Nombre, usuario.Rol.Nombre, permisos);

            return new AuthResponseDto(usuario.Id, token, usuario.Nombre, usuario.Rol.Nombre, permisos);
        }
    }
}