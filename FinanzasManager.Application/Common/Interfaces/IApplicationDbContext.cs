using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Usuario> Usuarios { get; set; }
        DbSet<Rol> Roles { get; }
        DbSet<Permiso> Permisos { get; }
        DbSet<RolPermiso> RolPermisos { get; }
        DbSet<Cliente> Clientes { get; }
        DbSet<Prestamo> Prestamos { get; }
        DbSet<Contrato> Contratos { get; }
        DbSet<Propiedad> Propiedades { get; }
        DbSet<TipoPropiedad> TipoPropiedades { get; }
        DbSet<MetodoPago> MetodosPago { get; }
        DbSet<Pago> Pagos { get; }
        DbSet<PagoDetalle> PagoDetalles { get; }

        // Este es el contrato para guardar cambios en la base de datos de forma asíncrona
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}