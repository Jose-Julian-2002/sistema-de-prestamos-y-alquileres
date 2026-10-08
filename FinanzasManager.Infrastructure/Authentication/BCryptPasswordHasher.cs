using FinanzasManager.Application.Common.Interfaces;

namespace FinanzasManager.Infrastructure.Authentication
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 11);
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return false;

            // Los hashes BCrypt válidos empiezan con "$2". Si no tiene ese formato
            // (ej. contraseñas antiguas en texto plano), fallamos limpio sin lanzar excepción.
            if (!hash.StartsWith("$2"))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
