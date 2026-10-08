using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class Propiedad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string CodigoLuz { get; set; } = string.Empty;
        public string CodigoAgua { get; set; } = string.Empty;
        public string? CodigoGas { get; set; }

        // Borrado logico: una propiedad con contratos activos no se elimina, se desactiva.
        public bool Estado { get; set; } = true;

        public int TipoPropiedadId { get; set; }
        public TipoPropiedad TipoPropiedad { get; set; } = null!;
        public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();

        // Constructor para Entity Framework
        private Propiedad() { }

        public Propiedad(
            int tipoPropiedadId,
            string nombre,
            string? direccion,
            string codigoLuz,
            string codigoAgua,
            string? codigoGas)
        {
            TipoPropiedadId = tipoPropiedadId;
            Nombre = nombre.Trim();
            Direccion = Normalizar(direccion);
            CodigoLuz = codigoLuz.Trim();
            CodigoAgua = codigoAgua.Trim();
            CodigoGas = Normalizar(codigoGas);
            Estado = true;
        }

        public void Actualizar(
            int tipoPropiedadId,
            string nombre,
            string? direccion,
            string codigoLuz,
            string codigoAgua,
            string? codigoGas)
        {
            TipoPropiedadId = tipoPropiedadId;
            Nombre = nombre.Trim();
            Direccion = Normalizar(direccion);
            CodigoLuz = codigoLuz.Trim();
            CodigoAgua = codigoAgua.Trim();
            CodigoGas = Normalizar(codigoGas);
        }

        private static string? Normalizar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}