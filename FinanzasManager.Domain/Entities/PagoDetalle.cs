using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    // Reparte un Pago entre los prestamos y contratos que cubre.
    // PrestamoId y ContratoId son opcionales: un detalle aplica a uno, al otro, o a ambos.
    public class PagoDetalle
    {
        public int Id { get; set; }
        public decimal MontoAplicado { get; set; }
        public DateTime FechaPago { get; set; }
        public int PagoId { get; set; }
        public int? PrestamoId { get; set; }
        public int? ContratoId { get; set; }

        public Pago Pago { get; set; } = null!;
        public Prestamo? Prestamo { get; set; }
        public Contrato? Contrato { get; set; }

        // Constructor para Entity Framework
        private PagoDetalle() { }

        public PagoDetalle(int pagoId, int? prestamoId, int? contratoId, decimal montoAplicado, DateTime fechaPago)
        {
            PagoId = pagoId;
            PrestamoId = prestamoId;
            ContratoId = contratoId;
            MontoAplicado = montoAplicado;
            FechaPago = fechaPago;
        }
    }
}