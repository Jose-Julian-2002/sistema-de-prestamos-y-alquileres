using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class TipoPropiedad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Propiedad> Propiedades { get; set; } = new List<Propiedad>();

        // Constructor para Entity Framework
        private TipoPropiedad() { }

        public TipoPropiedad(string nombre)
        {
            Nombre = nombre.Trim();
        }

        public void Actualizar(string nombre)
        {
            Nombre = nombre.Trim();
        }
    }
}