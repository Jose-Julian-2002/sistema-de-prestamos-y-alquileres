using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanzasManager.Domain.Entities
{
    public enum TipoTransaccion
    {
        Desembolso = 1,
        CargoInteres = 2,
        PagoInteres = 3,
        PagoCapital = 4,
    }
}
