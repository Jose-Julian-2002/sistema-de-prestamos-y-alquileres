using System;
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
    public record CrearRolCommand(string Nombre) : IRequest<int>;

    public class CrearRolCommandHandler : IRequestHandler<CrearRolCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CrearRolCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearRolCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del rol es obligatorio.");

            var nombre = request.Nombre.Trim();

            var existe = await _context.Roles.AnyAsync(r => r.Nombre == nombre, cancellationToken);

            if (existe)
                throw new InvalidOperationException($"Ya existe un rol con el nombre '{nombre}'.");

            var rol = new Rol(nombre);

            await _context.Roles.AddAsync(rol, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return rol.Id;
        }
    }

    public record ActualizarRolCommand(int Id, string Nombre) : IRequest<Unit>;

    public class ActualizarRolCommandHandler : IRequestHandler<ActualizarRolCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarRolCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ActualizarRolCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del rol es obligatorio.");

            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (rol is null)
                throw new InvalidOperationException($"No existe un rol con el Id {request.Id}.");

            var nombre = request.Nombre.Trim();

            // El nombre del rol viaja en el claim de role del token. Renombrar uno base
            // invalidaria los permisos ya sembrados y dejaria el registro publico sin rol
            // por defecto, asi que no se permite.
            var rolesBase = await ConsultasRol.NombresRolesBaseAsync(_context, cancellationToken);

            if (rolesBase.Contains(rol.Nombre) && rol.Nombre != nombre)
            {
                throw new InvalidOperationException(
                    $"No se puede renombrar el rol base '{rol.Nombre}' porque el sistema lo necesita con ese nombre.");
            }

            var existe = await _context.Roles
                .AnyAsync(r => r.Nombre == nombre && r.Id != request.Id, cancellationToken);

            if (existe)
                throw new InvalidOperationException($"Ya existe un rol con el nombre '{nombre}'.");

            rol.Nombre = nombre;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

    public record EliminarRolCommand(int Id) : IRequest<Unit>;

    public class EliminarRolCommandHandler : IRequestHandler<EliminarRolCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public EliminarRolCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EliminarRolCommand request, CancellationToken cancellationToken)
        {
            var rol = await _context.Roles
                .Include(r => r.Permisos)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (rol is null)
                throw new InvalidOperationException($"No existe un rol con el Id {request.Id}.");

            // Los tres roles base sostienen el funcionamiento del sistema:
            // Administrador protege la administracion y Cobrador es el rol por
            // defecto del registro publico.
            var rolesBase = await ConsultasRol.NombresRolesBaseAsync(_context, cancellationToken);

            if (rolesBase.Contains(rol.Nombre))
            {
                throw new InvalidOperationException(
                    $"No se puede eliminar el rol base '{rol.Nombre}'.");
            }

            var usuariosConElRol = await _context.Usuarios
                .CountAsync(u => u.RolId == request.Id, cancellationToken);

            if (usuariosConElRol > 0)
            {
                throw new InvalidOperationException(
                    $"No se puede eliminar '{rol.Nombre}' porque tiene {usuariosConElRol} usuario(s) asignado(s).");
            }

            // Las asignaciones de permisos van en cascada por el DeleteBehavior.
            _context.RolPermisos.RemoveRange(rol.Permisos);
            _context.Roles.Remove(rol);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}