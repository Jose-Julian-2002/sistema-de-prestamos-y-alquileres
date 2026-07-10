using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanzasManager.Domain.Enums;

namespace FinanzasManager.Domain.Entities
{
    public class TransaccionPrestamo
    {
        public Guid Id { get; set; }
        public Guid PrestamoId { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public TipoTransaccion Tipo { get; set; }
        public string Nota { get; set; } = string.Empty;
        public TransaccionPrestamo(decimal monto, TipoTransaccion tipo, string nota)
        {
           Id = Guid.NewGuid();
            Monto = monto;
            Tipo = tipo;
            Fecha = DateTime.Now;
            Nota = nota;
        }
    }
}
