using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class MetodoPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Constructor para Entity Framework
        private MetodoPago() { }

        public MetodoPago(string nombre)
        {
            Nombre = nombre.Trim();
        }

        public void Actualizar(string nombre)
        {
            Nombre = nombre.Trim();
        }
    }
}