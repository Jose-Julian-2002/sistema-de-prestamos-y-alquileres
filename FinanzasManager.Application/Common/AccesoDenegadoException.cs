using System;

namespace FinanzasManager.Application.Common
{
    // El usuario esta autenticado, pero su rol (o la antiguedad del registro)
    // no le permite ejecutar la accion. Se traduce a 403; el 401 se reserva
    // para credenciales invalidas.
    public class AccesoDenegadoException : Exception
    {
        public AccesoDenegadoException(string mensaje) : base(mensaje)
        {
        }
    }
}
