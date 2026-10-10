using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanzasManager.Application.Features.Prestamos
{
    // Regla de la casa para calcular las cuotas de un prestamo.
    //
    // El interes es MENSUAL y se calcula SIEMPRE sobre el capital inicial, no sobre
    // el saldo pendiente. Eso hace que la cuota sea fija todos los meses:
    //
    //   Abono de capital mensual = MontoCapital / TotalCuotas
    //   Interes mensual en dinero = MontoCapital x (Interes / 100)
    //   Cuota mensual            = AbonoCapital + Interes mensual
    //   Total intereses          = Interes mensual x TotalCuotas
    //   Total a pagar            = MontoCapital + Total intereses
    //
    // Ejemplo: 10.000 Bs a 10 cuotas con 5% mensual
    //   Abono capital = 1.000, Interes = 500, Cuota = 1.500, Total = 15.000
    public static class CalculosPrestamo
    {
        public static PrestamoResultado Calcular(decimal montoCapital, decimal interes, decimal totalCuotas)
        {
            var abonoCapital = Redondear(montoCapital / totalCuotas);
            var interesMensual = Redondear(montoCapital * (interes / 100m));
            var cuotaMensual = Redondear(abonoCapital + interesMensual);
            var totalIntereses = Redondear(interesMensual * totalCuotas);
            var totalPagar = Redondear(montoCapital + totalIntereses);

            return new PrestamoResultado(abonoCapital, interesMensual, cuotaMensual, totalIntereses, totalPagar);
        }

        public static IReadOnlyList<CuotaPrestamo> GenerarCuotas(
            decimal montoCapital,
            decimal interes,
            decimal totalCuotas)
        {
            var resultado = Calcular(montoCapital, interes, totalCuotas);
            var cantidad = (int)totalCuotas;

            var cuotas = new List<CuotaPrestamo>(cantidad);
            for (var i = 1; i <= cantidad; i++)
            {
                cuotas.Add(new CuotaPrestamo(
                    i,
                    resultado.AbonoCapitalMensual,
                    resultado.InteresMensual,
                    resultado.CuotaMensual));
            }

            return cuotas;
        }

        // Medio al alza (AwayFromZero) para que la cuota no pierda centavos a favor del cliente.
        private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
    }

    public record PrestamoResultado(
        decimal AbonoCapitalMensual,
        decimal InteresMensual,
        decimal CuotaMensual,
        decimal TotalIntereses,
        decimal TotalPagar);

    public record CuotaPrestamo(int Numero, decimal AbonoCapital, decimal Interes, decimal Total);
}