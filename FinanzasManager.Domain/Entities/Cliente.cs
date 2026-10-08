using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? DocumentoIdentidad { get; set; }
        public string? Telefono { get; set; }
        public DateTime? FechaRegistro { get; set; }

        // Borrado logico: un cliente con prestamos o contratos se desactiva en lugar
        // de eliminarse, para que su historial siga siendo trazable.
        public bool Estado { get; set; } = true;

        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
        public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();

        // Constructor para Entity Framework
        private Cliente() { }

        public Cliente(string nombre, string? documentoIdentidad, string? telefono)
        {
            Nombre = nombre.Trim();
            DocumentoIdentidad = Normalizar(documentoIdentidad);
            Telefono = Normalizar(telefono);
            FechaRegistro = DateTime.Now;
            Estado = true;
        }

        // El documento alimenta un indice unico. Sin normalizar, un cliente con
        // "123" y otro con " 123 " estarian duplicando documento: pasarian el filtro
        // en la aplicacion pero fallarian al llegar a la base.
        public void Actualizar(string nombre, string? documentoIdentidad, string? telefono)
        {
            Nombre = nombre.Trim();
            DocumentoIdentidad = Normalizar(documentoIdentidad);
            Telefono = Normalizar(telefono);
        }

        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}