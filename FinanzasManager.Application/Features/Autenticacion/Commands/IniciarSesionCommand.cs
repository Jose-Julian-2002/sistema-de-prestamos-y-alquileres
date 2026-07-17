using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Domain.Entities;
using FinanzasManager.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FinanzasManager.Application.Features.Autenticacion.Commands
{
    public record IniciarSesionCommand(string Email, string Password): IRequest<AuthResponseDto>;
    public record AuthResponseDto(string Token, string Nombre, string Rol);
    public class IniciarSesionCommandHandler : IRequestHandler<IniciarSesionCommand, AuthResponseDto>
    {
        private readonly IApplicationDbContext _context;
        // NOTA: Para producción usaremos hashing (como BCrypt), por ahora validamos texto plano
        // ya que el admin de prueba lo insertamos como 'admin123'

        public IniciarSesionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuthResponseDto> Handle(IniciarSesionCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscar al usuario por Email
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Activo, cancellationToken);

            // 2. Si no existe o la contraseña no coincide (Control de errores básico de seguridad)
            if (usuario == null || usuario.PasswordHash != request.Password)
            {
                throw new Exception("Credenciales incorrectas o usuario inactivo.");
            }

            // 3. Generar el Token JWT (En el paso siguiente crearemos el helper, por ahora simulamos la firma)
            // Para mantener el handler limpio, la lógica del token la inyectaremos o la procesamos aquí:
            string tokenGenerado = "TOKEN_JWT_TEMPORAL_PROVISORIO";

            // TODO: Reemplazar por tu generador de JWT real (lo configuramos abajo)

            return new AuthResponseDto(tokenGenerado, usuario.Nombre, usuario.Rol);
        }
    }
}
