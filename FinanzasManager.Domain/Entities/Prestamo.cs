using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class Prestamo
    {
        public int Id { get; set; }
        public decimal MontoCapital { get; set; }

        // Porcentaje de interes aplicado al capital
        public decimal Interes { get; set; }

        // Importe de cada cuota, no la cantidad de cuotas
        public decimal TotalCuotas { get; set; }

        public DateTime FechaInicio { get; set; }
        public bool Estado { get; set; } = true;
        public int ClienteId { get; set; }

        public Cliente Cliente { get; set; } = null!;
        public ICollection<PagoDetalle> PagoDetalles { get; set; } = new List<PagoDetalle>();

        // Constructor para Entity Framework
        private Prestamo() { }

        public Prestamo(int clienteId, decimal montoCapital, decimal interes, decimal totalCuotas, DateTime fechaInicio)
        {
            ClienteId = clienteId;
            MontoCapital = montoCapital;
            Interes = interes;
            TotalCuotas = totalCuotas;
            FechaInicio = fechaInicio;
            Estado = true;
        }
    }
}