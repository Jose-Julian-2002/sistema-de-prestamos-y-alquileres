using System;

namespace FinanzasManager.Domain.Entities
{
    // Tabla puente que resuelve la relacion N:M entre Rol y Permiso.
    // La clave primaria es compuesta: (RolId, PermisoId).
    public class RolPermiso
    {
        public int RolId { get; set; }
        public int PermisoId { get; set; }

        public Rol Rol { get; set; } = null!;
        public Permiso Permiso { get; set; } = null!;

        // Constructor para Entity Framework
        private RolPermiso() { }

        public RolPermiso(int rolId, int permisoId)
        {
            RolId = rolId;
            PermisoId = permisoId;
        }
    }
}