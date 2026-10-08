using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Usuarios
{
    // Reglas de seguridad que comparten los comandos de administracion de usuarios.
    public static class ValidacionesUsuario
    {
        // Nadie se desactiva ni se elimina a si mismo: dejaria al sistema sin nadie
        // que pueda revertir la operacion.
        public static void NoEsElPropio(int usuarioId, int? solicitanteId, string operacion)
        {
            if (solicitanteId == usuarioId)
                throw new InvalidOperationException($"No puedes {operacion} tu propia cuenta.");
        }

        // Un rol es de administracion si tiene asignado el permiso
        // Usuarios.GestionarRoles. Se define por el permiso y no por el nombre del rol,
        // porque asi el criterio sigue siendo valido aunque se creen roles nuevos.
        private static IQueryable<Usuario> Administradores(IQueryable<Usuario> usuarios)
        {
            return usuarios.Where(u => u.Estado
                                       && u.Rol.Permisos.Any(rp => rp.Permiso.Nombre == Permisos.Usuarios.GestionarRoles));
        }

        // Indica si el rol indicado tiene el permiso de gestion de roles.
        public static Task<bool> EsRolDeAdministracion(IQueryable<Rol> roles, int rolId, CancellationToken cancellationToken)
        {
            return roles.AnyAsync(r => r.Id == rolId
                                       && r.Permisos.Any(rp => rp.Permiso.Nombre == Permisos.Usuarios.GestionarRoles),
                    cancellationToken);
        }

        // Comprueba si el usuario es el ultimo administrador activo del sistema.
        // Si lo es, degradarlo (desactivarlo, borrarlo o quitarle el rol) dejaria
        // la base sin acceso de gestion.
        public static async Task EsElUltimoAdministrador(
            IApplicationDbContext context,
            int usuarioId,
            CancellationToken cancellationToken)
        {
            var usuarios = context.Usuarios.AsNoTracking();

            var esAdministrador = await Administradores(usuarios)
                .AnyAsync(u => u.Id == usuarioId, cancellationToken);

            if (!esAdministrador)
                return;

            var hayOtros = await Administradores(usuarios)
                .AnyAsync(u => u.Id != usuarioId, cancellationToken);

            if (!hayOtros)
            {
                throw new InvalidOperationException(
                    "No se puede desactivar, eliminar ni cambiar el rol del único administrador. Crea o activa otro administrador primero.");
            }
        }
    }
}