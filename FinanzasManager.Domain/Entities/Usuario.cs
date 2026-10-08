using System;
using System.Collections.Generic;

namespace FinanzasManager.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Gmail { get; set; } = string.Empty;

        // Hash BCrypt de la contrasena. NUNCA la contrasena en texto plano.
        public string Password { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;
        public int RolId { get; set; }

        public Rol Rol { get; set; } = null!;

        // Historial de lo que el usuario ha gestionado
        public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Constructor para Entity Framework
        private Usuario() { }

        public Usuario(int rolId, string nombre, string gmail, string password)
        {
            RolId = rolId;
            Nombre = nombre.Trim();
            Gmail = gmail.ToLower().Trim();
            Password = password;
            Estado = true;
        }
    }
}