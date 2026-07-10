using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanzasManager.Domain.Entities
{
    public class Prestamo
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public decimal MontoCapitalInicial { get; set; }
        public decimal PorcentajeInteres { get; set; } 
        public DateTime FechaDesembolso { get; set; }
        public bool Activo { get; set; }

        // Lista de movimientos
        public List<TransaccionPrestamo> Transacciones { get; private set; } = new();

        public Prestamo(Guid clienteId, decimal montoCapitalInicial, decimal porcentajeInteres)
        {
            Id = Guid.NewGuid();
            ClienteId = clienteId;
            MontoCapitalInicial = montoCapitalInicial;
            PorcentajeInteres = porcentajeInteres;
            FechaDesembolso = DateTime.Now;
            Activo = true;

            // Regla de Negocio Al crear un préstamo, automáticamente se genera la transacción de desembolso
            Transacciones.Add(new TransaccionPrestamo(montoCapitalInicial, TipoTransaccion.Desembolso, "Desembolso inicial del préstamo"));
        }

        //registrar pagos
        public void RegistrarPago(decimal monto, TipoTransaccion tipo, string nota)
        {
            if (monto <= 0) throw new ArgumentException("El monto debe ser mayor a cero.");

            Transacciones.Add(new TransaccionPrestamo(monto, tipo, nota));
        }

        //cálculo dinámico que garantiza que todo cuadre
        public decimal CalcularSaldoActual()
        {
            decimal saldo = 0;
            foreach (var tx in Transacciones)
            {
                if (tx.Tipo == TipoTransaccion.Desembolso || tx.Tipo == TipoTransaccion.CargoInteres)
                    saldo += tx.Monto;
                else if (tx.Tipo == TipoTransaccion.PagoInteres || tx.Tipo == TipoTransaccion.PagoCapital)
                    saldo -= tx.Monto;
            }
            return saldo;

        }
    }
}
