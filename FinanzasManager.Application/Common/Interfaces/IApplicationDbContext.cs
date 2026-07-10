using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace FinanzasManager.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Cliente> Clientes { get; }
        DbSet<Prestamo> Prestamos { get; }
        DbSet<TransaccionPrestamo> TransaccionesPrestamos { get; }

        // Este es el contrato para guardar cambios en la base de datos de forma asíncrona
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
