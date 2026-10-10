using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class Contrato
    {
        public int Id { get; set; }
        public decimal MontoAlquiler { get; set; }
        public decimal MontoGarantia { get; set; }

        // Fecha en la que se paga cada mes. La columna es date, no el dia del mes.
        public DateTime DiaPago { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Estado { get; set; } = true;

        public int PropiedadId { get; set; }
        public int ClienteId { get; set; }
        public int UsuarioId { get; set; }

        public Propiedad Propiedad { get; set; } = null!;
        public Cliente Cliente { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public ICollection<PagoDetalle> PagoDetalles { get; set; } = new List<PagoDetalle>();

        // Constructor para Entity Framework
        private Contrato() { }

        public Contrato(
            int propiedadId,
            int clienteId,
            int usuarioId,
            decimal montoAlquiler,
            decimal montoGarantia,
            DateTime diaPago,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            PropiedadId = propiedadId;
            ClienteId = clienteId;
            UsuarioId = usuarioId;
            MontoAlquiler = montoAlquiler;
            MontoGarantia = montoGarantia;
            DiaPago = diaPago;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
            Estado = true;
        }

        // El cobrador que registro el contrato no cambia al editarlo, por eso
        // UsuarioId no forma parte de los datos editables.
        public void Actualizar(
            int propiedadId,
            int clienteId,
            decimal montoAlquiler,
            decimal montoGarantia,
            DateTime diaPago,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            PropiedadId = propiedadId;
            ClienteId = clienteId;
            MontoAlquiler = montoAlquiler;
            MontoGarantia = montoGarantia;
            DiaPago = diaPago;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
        }
    }
}