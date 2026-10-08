using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Roles.Queries
{
    // Permiso disponible del sistema, con la informacion que necesita el frontend
    // para agrupar y describir cada permiso.
    public record PermisoCatalogoDto(
        int Id,
        string Nombre,
        string Modulo,
        string Accion,
        string Descripcion);

    public record ListarPermisosQuery(string? Buscar = null) : IRequest<IReadOnlyList<PermisoCatalogoDto>>;

    public class ListarPermisosQueryHandler : IRequestHandler<ListarPermisosQuery, IReadOnlyList<PermisoCatalogoDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListarPermisosQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<PermisoCatalogoDto>> Handle(
            ListarPermisosQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Permisos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Buscar))
            {
                var termino = request.Buscar.Trim().ToLower();
                query = query.Where(p => p.Nombre.ToLower().Contains(termino));
            }

            var permisos = await query
                .OrderBy(p => p.Nombre)
                .Select(p => new { p.Id, p.Nombre })
                .ToListAsync(cancellationToken);

            return permisos
                .Select(p => new PermisoCatalogoDto(
                    p.Id,
                    p.Nombre,
                    ObtenerModulo(p.Nombre),
                    ObtenerAccion(p.Nombre),
                    ObtenerDescripcion(p.Nombre)))
                .ToList();
        }

        // Los nombres siguen el patron "Modulo.Accion", ver Permisos.cs.
        private static string ObtenerModulo(string nombre)
        {
            var separador = nombre.IndexOf('.');
            return separador > 0 ? nombre[..separador] : nombre;
        }

        private static string ObtenerAccion(string nombre)
        {
            var separador = nombre.IndexOf('.');
            return separador > 0 ? nombre[(separador + 1)..] : nombre;
        }

        private static string ObtenerDescripcion(string nombre) => nombre switch
        {
            Permisos.Clientes.Ver => "Ver la lista y el detalle de clientes",
            Permisos.Clientes.Crear => "Registrar clientes nuevos",
            Permisos.Clientes.Editar => "Modificar los datos de un cliente",
            Permisos.Clientes.Eliminar => "Dar de baja clientes",

            Permisos.Prestamos.Ver => "Ver la lista y el detalle de prestamos",
            Permisos.Prestamos.Crear => "Registrar prestamos nuevos",
            Permisos.Prestamos.Editar => "Modificar las condiciones de un prestamo",
            Permisos.Prestamos.Abonos => "Registrar abonos y pagos de prestamos",

            Permisos.Contratos.Ver => "Ver la lista y el detalle de contratos",
            Permisos.Contratos.Crear => "Registrar contratos de alquiler",
            Permisos.Contratos.Editar => "Modificar contratos de alquiler",

            Permisos.Propiedades.Ver => "Ver la lista y el detalle de propiedades",
            Permisos.Propiedades.Crear => "Registrar propiedades nuevas",
            Permisos.Propiedades.Editar => "Modificar los datos de una propiedad",

            Permisos.Pagos.Ver => "Ver la lista y el detalle de pagos",
            Permisos.Pagos.Crear => "Registrar pagos",

            Permisos.Usuarios.Ver => "Ver la lista y el detalle de usuarios",
            Permisos.Usuarios.Crear => "Crear usuarios",
            Permisos.Usuarios.Editar => "Modificar usuarios, activarlos y cambiar su contrasena",
            Permisos.Usuarios.GestionarRoles => "Cambiar el rol de un usuario y administrar roles y permisos",

            Permisos.Roles.Ver => "Ver la lista y el detalle de roles",
            Permisos.Roles.Crear => "Crear roles nuevos",
            Permisos.Roles.Editar => "Renombrar roles",
            Permisos.Roles.Eliminar => "Eliminar roles que no sean base",
            Permisos.Roles.AsignarPermisos => "Asignar y quitar permisos a un rol",

            Permisos.ReportesVer => "Consultar reportes",

            _ => nombre
        };
    }
}