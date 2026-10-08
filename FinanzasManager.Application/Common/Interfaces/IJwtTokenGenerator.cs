using System;
using System.Collections.Generic;

namespace FinanzasManager.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerarToken(int usuarioId, string nombre, string rol, IEnumerable<string> permisos);
    }
}