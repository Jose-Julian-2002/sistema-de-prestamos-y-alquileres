using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Permisos que tiene asignados este rol a traves de la tabla puente RolPermiso
        public ICollection<RolPermiso> Permisos { get; set; } = new List<RolPermiso>();

        // Constructor para Entity Framework
        private Rol() { }

        public Rol(string nombre)
        {
            Nombre = nombre.Trim();
        }
    }
}