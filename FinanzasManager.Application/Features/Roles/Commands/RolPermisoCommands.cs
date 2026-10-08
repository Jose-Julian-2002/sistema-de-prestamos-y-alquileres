using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using FinanzasManager.Application.Features.Roles.Queries;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Roles.Commands
{
    // Reemplaza el conjunto completo de permisos de un rol. Es lo que envia un
    // formulario con checkboxes al guardar.
    public record AsignarPermisosRolCommand(int RolId, IReadOnlyList<int> PermisoIds) : IRequest<Unit>;

    public class AsignarPermisosRolCommandHandler : IRequestHandler<AsignarPermisosRolCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public AsignarPermisosRolCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(AsignarPermisosRolCommand request, CancellationToken cancellationToken)
        {
            var rol = await _context.Roles
                .Include(r => r.Permisos)
                .FirstOrDefaultAsync(r => r.Id == request.RolId, cancellationToken);

            if (rol is null)
                throw new InvalidOperationException($"No existe un rol con el Id {request.RolId}.");

            var deseados = request.PermisoIds?.Distinct().ToList() ?? new List<int>();

            // Validamos que todos los ids existan: si el frontend manda un permiso
            // que no esta en la base, preferimos fallar a crear filas huerfanas.
            if (deseados.Count > 0)
            {
                var existen = await _context.Permisos
                    .Where(p => deseados.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                var inexistentes = deseados.Except(existen).ToList();

                if (inexistentes.Count > 0)
                    throw new ArgumentException($"Permiso(s) inexistente(s): {string.Join(", ", inexistentes)}.");
            }

            await VerificarQueQuedaAlguienQueGestione(_context, rol, deseados, cancellationToken);

            var actuales = rol.Permisos.Select(rp => rp.PermisoId).ToList();

            var aQuitar = actuales.Except(deseados).ToList();
            if (aQuitar.Count > 0)
            {
                var filasAQuitar = rol.Permisos.Where(rp => aQuitar.Contains(rp.PermisoId)).ToList();
                _context.RolPermisos.RemoveRange(filasAQuitar);
            }

            var yaAsignados = actuales.ToHashSet();
            var aAgregar = deseados.Where(id => !yaAsignados.Contains(id)).ToList();

            if (aAgregar.Count > 0)
                await _context.RolPermisos.AddRangeAsync(aAgregar.Select(id => new RolPermiso(rol.Id, id)), cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        // Si el permiso de gestionar usuarios se queda sin ningun rol que lo tenga,
        // el sistema queda sin nadie capaz de administrar usuarios ni roles.
        private static async Task VerificarQueQuedaAlguienQueGestione(
            IApplicationDbContext context,
            Rol rol,
            IReadOnlyCollection<int> deseados,
            CancellationToken cancellationToken)
        {
            var nombreGestionar = Permisos.Usuarios.GestionarRoles;

            var permisoGestionar = await context.Permisos
                .Where(p => p.Nombre == nombreGestionar)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (permisoGestionar is null)
                return;

            // Este rol seguira teniendo el permiso?
            if (deseados.Contains(permisoGestionar.Value))
                return;

            // Hay otro rol que lo conserve?
            var otroQueLoTiene = await context.RolPermisos
                .AnyAsync(rp => rp.PermisoId == permisoGestionar.Value
                             && rp.RolId != rol.Id,
                    cancellationToken);

            if (!otroQueLoTiene)
            {
                throw new InvalidOperationException(
                    $"No se puede quitar '{nombreGestionar}': ningun rol lo tendria y el sistema quedaria sin administracion.");
            }
        }
    }

    // Quita un permiso concreto sin tocar el resto.
    public record QuitarPermisoRolCommand(int RolId, int PermisoId) : IRequest<Unit>;

    public class QuitarPermisoRolCommandHandler : IRequestHandler<QuitarPermisoRolCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public QuitarPermisoRolCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(QuitarPermisoRolCommand request, CancellationToken cancellationToken)
        {
            var asignacion = await _context.RolPermisos
                .Include(rp => rp.Permiso)
                .Include(rp => rp.Rol)
                .FirstOrDefaultAsync(rp => rp.RolId == request.RolId && rp.PermisoId == request.PermisoId, cancellationToken);

            if (asignacion is null)
                throw new InvalidOperationException("El rol no tiene asignado ese permiso.");

            if (asignacion.Permiso.Nombre == Permisos.Usuarios.GestionarRoles)
            {
                var otroQueLoTiene = await _context.RolPermisos
                    .AnyAsync(rp => rp.PermisoId == request.PermisoId && rp.RolId != request.RolId, cancellationToken);

                if (!otroQueLoTiene)
                {
                    throw new InvalidOperationException(
                        $"No se puede quitar '{Permisos.Usuarios.GestionarRoles}': " +
                        "ningun rol lo tendria y el sistema quedaria sin administracion.");
                }
            }

            _context.RolPermisos.Remove(asignacion);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}