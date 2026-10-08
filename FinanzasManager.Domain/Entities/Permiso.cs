using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class Permiso
    {
        public int Id { get; set; }

        // Nombre tecnico del permiso, por ejemplo "Clientes.Crear".
        // Es el valor que viaja como claim en el token y el que exigen las policies.
        public string Nombre { get; set; } = string.Empty;

        // Roles que tienen este permiso asignado
        public ICollection<RolPermiso> Roles { get; set; } = new List<RolPermiso>();

        // Constructor para Entity Framework
        private Permiso() { }

        public Permiso(string nombre)
        {
            Nombre = nombre.Trim();
        }
    }
}