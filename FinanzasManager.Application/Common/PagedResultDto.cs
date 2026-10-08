using System.Collections.Generic;

namespace FinanzasManager.Application.Common
{
    // Envoltura comun de los listados paginados. Todos los listados de la API
    //devuelven esta misma forma para que el frontend tenga un solo contrato.
    public record PagedResultDto<T>(IReadOnlyList<T> Items, int Total, int Pagina, int Tamano);
}