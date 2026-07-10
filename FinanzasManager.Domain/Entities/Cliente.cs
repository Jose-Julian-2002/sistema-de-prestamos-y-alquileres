using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanzasManager.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string DocumentoIdentidad { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public Cliente(string nombre, string documentoIdentidad, string telefono)
        {
            Id = Guid.NewGuid();
            Nombre = nombre;
            DocumentoIdentidad = documentoIdentidad;
            Telefono = telefono;
        }
    }
}
