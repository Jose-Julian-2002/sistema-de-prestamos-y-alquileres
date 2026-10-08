using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class Pago
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal MontoTotal { get; set; }
        public int MetodoPagoId { get; set; }
        public int UsuarioId { get; set; }

        public MetodoPago MetodoPago { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public ICollection<PagoDetalle> Detalles { get; set; } = new List<PagoDetalle>();

        // Constructor para Entity Framework
        private Pago() { }

        public Pago(int metodoPagoId, int usuarioId, DateTime fecha, decimal montoTotal)
        {
            MetodoPagoId = metodoPagoId;
            UsuarioId = usuarioId;
            Fecha = fecha;
            MontoTotal = montoTotal;
        }
    }
}