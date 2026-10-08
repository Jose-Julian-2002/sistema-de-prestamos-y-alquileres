using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FinanzasManager.Application.Common;
using FinanzasManager.Domain.Entities;
using FinanzasManager.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Infrastructure.Persistence
{
    // Siembra los permisos y los roles base del sistema. Es idempotente:
    // se puede ejecutar en cada arranque sin duplicar nada.
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            // 1. Permisos del sistema
            var permisosExistentes = await context.Permisos
                .Select(p => p.Nombre)
                .ToListAsync(cancellationToken);

            var permisosNuevos = Permisos.Todos
                .Where(nombre => !permisosExistentes.Contains(nombre))
                .Select(nombre => new Permiso(nombre))
                .ToList();

            if (permisosNuevos.Count > 0)
                await context.Permisos.AddRangeAsync(permisosNuevos);

            await context.SaveChangesAsync(cancellationToken);

            // 2. Roles base con sus permisos
            await SeedRolAsync(context, Permisos.NombreRolAdministrador, Permisos.DeAdministrador, cancellationToken);
            await SeedRolAsync(context, Permisos.NombreRolCobrador, Permisos.DeCobrador, cancellationToken);
            await SeedRolAsync(context, Permisos.NombreRolCliente, Permisos.DeCliente, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            // 3. Administrador inicial, solo si la tabla de usuarios sigue vacia.
            // Sin esto nadie podria crear usuarios ni asignar roles, porque el registro
            // publico siempre asigna el rol Cobrador.
            await SeedAdministradorInicialAsync(context, cancellationToken);
        }

        // Credenciales del administrador inicial. Deben cambiarse en el primer ingreso.
        public const string AdminInicialGmail = "admin@finanzas.com";
        private const string AdminInicialPassword = "admin123";

        private static async Task SeedAdministradorInicialAsync(
            ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            if (await context.Usuarios.AnyAsync(cancellationToken))
                return;

            var rolAdmin = await context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == Permisos.NombreRolAdministrador, cancellationToken);

            if (rolAdmin is null)
                return;

            // El hash se genera con BCrypt, nunca se guarda la contrasena en claro.
            var hasher = new BCryptPasswordHasher();

            var admin = new Usuario(
                rolAdmin.Id,
                "Administrador",
                AdminInicialGmail,
                hasher.Hash(AdminInicialPassword));

            await context.Usuarios.AddAsync(admin, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            Console.WriteLine(
                $"[AVISO] Se creo el administrador inicial '{AdminInicialGmail}' " +
                "con la contrasena por defecto. Cambiala en el primer ingreso.");
        }

        private static async Task SeedRolAsync(
            ApplicationDbContext context,
            string nombreRol,
            IReadOnlyList<string> nombresPermisos,
            CancellationToken cancellationToken)
        {
            var rol = await context.Roles
                .Include(r => r.Permisos)
                .FirstOrDefaultAsync(r => r.Nombre == nombreRol, cancellationToken);

            if (rol is null)
            {
                rol = new Rol(nombreRol);
                await context.Roles.AddAsync(rol, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            var yaAsignados = rol.Permisos.Select(rp => rp.PermisoId).ToHashSet();

            var permisos = await context.Permisos
                .Where(p => nombresPermisos.Contains(p.Nombre))
                .ToListAsync(cancellationToken);

            foreach (var permiso in permisos)
            {
                if (yaAsignados.Contains(permiso.Id))
                    continue;

                rol.Permisos.Add(new RolPermiso(rol.Id, permiso.Id));
            }
        }
    }
}