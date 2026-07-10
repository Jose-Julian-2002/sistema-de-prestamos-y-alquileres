using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanzasManager.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Prestamo> Prestamos => Set<Prestamo>();
        public DbSet<TransaccionPrestamo> TransaccionesPrestamos => Set<TransaccionPrestamo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // (Aquí mantienes las configuraciones de Fluent API de Clientes, Prestamos y Transacciones que pusimos antes)
            modelBuilder.Entity<Cliente>(entity => {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(c => c.DocumentoIdentidad).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<Prestamo>(entity => {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.MontoCapitalInicial).HasColumnType("decimal(18,2)");
                entity.Property(p => p.PorcentajeInteres).HasColumnType("decimal(5,2)");
                entity.HasOne(p => p.Cliente).WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TransaccionPrestamo>(entity => {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Monto).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Nota).HasMaxLength(250);
                entity.HasOne<Prestamo>().WithMany(p => p.Transacciones).HasForeignKey(t => t.PrestamoId).OnDelete(DeleteBehavior.Cascade);
            });
        }

        // REGRA DE ORO: Implementamos el método que pide la interfaz mapeándolo al de DbContext
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
