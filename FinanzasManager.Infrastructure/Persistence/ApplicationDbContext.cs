using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Los DbSet van en plural por convencion, pero las tablas en la base estan
        // en singular. Por eso cada entidad declara su ToTable en OnModelCreating.
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<Permiso> Permisos => Set<Permiso>();
        public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Prestamo> Prestamos => Set<Prestamo>();
        public DbSet<Contrato> Contratos => Set<Contrato>();
        public DbSet<Propiedad> Propiedades => Set<Propiedad>();
        public DbSet<TipoPropiedad> TipoPropiedades => Set<TipoPropiedad>();
        public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
        public DbSet<Pago> Pagos => Set<Pago>();
        public DbSet<PagoDetalle> PagoDetalles => Set<PagoDetalle>();

        // Transaccion explicita para el alta de recibos: la hereda de DbContext pero
        // se declara aqui para satisfacer IApplicationDbContext con nombre completo.
        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => await Database.BeginTransactionAsync(cancellationToken);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Autorizacion: Rol, Permiso y su tabla puente
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).ValueGeneratedOnAdd();
                entity.Property(r => r.Nombre).HasMaxLength(100).IsRequired();
                entity.HasIndex(r => r.Nombre).IsUnique();
            });

            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.ToTable("Permiso");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
                entity.HasIndex(p => p.Nombre).IsUnique();
            });

            modelBuilder.Entity<RolPermiso>(entity =>
            {
                entity.ToTable("RolPermiso");
                entity.HasKey(rp => new { rp.RolId, rp.PermisoId });

                // Si se borra un rol se van sus asignaciones de permisos, y viceversa.
                entity.HasOne(rp => rp.Rol)
                      .WithMany(r => r.Permisos)
                      .HasForeignKey(rp => rp.RolId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permiso)
                      .WithMany(p => p.Roles)
                      .HasForeignKey(rp => rp.PermisoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 2. Usuarios
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                entity.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Gmail).HasMaxLength(200).IsRequired();
                entity.Property(u => u.Password).HasMaxLength(512).IsRequired();

                // El login busca por correo, la unicidad la garantiza el indice.
                entity.HasIndex(u => u.Gmail).IsUnique();

                entity.Property(u => u.Estado).HasDefaultValue(true);

                // Restrict: no se puede eliminar un rol que todavia tenga usuarios.
                entity.HasOne(u => u.Rol)
                      .WithMany()
                      .HasForeignKey(u => u.RolId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 3. Propiedades
            modelBuilder.Entity<TipoPropiedad>(entity =>
            {
                entity.ToTable("TipoPropiedad");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
                entity.Property(t => t.Nombre).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Propiedad>(entity =>
            {
                entity.ToTable("Propiedad");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Direccion).HasMaxLength(512);
                entity.Property(p => p.CodigoLuz).HasMaxLength(100).IsRequired();
                entity.Property(p => p.CodigoAgua).HasMaxLength(100).IsRequired();
                entity.Property(p => p.CodigoGas).HasMaxLength(100);
                entity.Property(p => p.Estado).HasDefaultValue(true);

                entity.HasOne(p => p.TipoPropiedad)
                      .WithMany(t => t.Propiedades)
                      .HasForeignKey(p => p.TipoPropiedadId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 4. Clientes
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Cliente");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.Nombre).HasMaxLength(300).IsRequired();
                entity.Property(c => c.DocumentoIdentidad).HasMaxLength(40);
                entity.Property(c => c.Telefono).HasMaxLength(40);
                entity.Property(c => c.Estado).HasDefaultValue(true);

                // El documento identifica a una persona, no puede repetirse.
                // Filtro IS NOT NULL porque en un indice unico de SQL Server dos NULL
                // cuentan como duplicados, y aqui los clientes sin documento son validos.
                entity.HasIndex(c => c.DocumentoIdentidad)
                      .IsUnique()
                      .HasFilter("[DocumentoIdentidad] IS NOT NULL");
            });

            // 5. Prestamos
            modelBuilder.Entity<Prestamo>(entity =>
            {
                entity.ToTable("Prestamo");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.MontoCapital).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Interes).HasColumnType("decimal(10,2)");
                entity.Property(p => p.TotalCuotas).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Estado).HasDefaultValue(true);

                // Restrict: no se deben borrar clientes con historial de prestamos.
                entity.HasOne(p => p.Cliente)
                      .WithMany(c => c.Prestamos)
                      .HasForeignKey(p => p.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 6. Contratos de alquiler
            modelBuilder.Entity<Contrato>(entity =>
            {
                entity.ToTable("Contrato");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
                entity.Property(c => c.MontoAlquiler).HasColumnType("decimal(10,2)");
                entity.Property(c => c.MontoGarantia).HasColumnType("decimal(10,2)");
                entity.Property(c => c.Estado).HasDefaultValue(true);

                entity.HasOne(c => c.Propiedad)
                      .WithMany(p => p.Contratos)
                      .HasForeignKey(c => c.PropiedadId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Cliente)
                      .WithMany(c => c.Contratos)
                      .HasForeignKey(c => c.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Usuario)
                      .WithMany(u => u.Contratos)
                      .HasForeignKey(c => c.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. Pagos
            modelBuilder.Entity<MetodoPago>(entity =>
            {
                entity.ToTable("MetodoPago");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Id).ValueGeneratedOnAdd();
                entity.Property(m => m.Nombre).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.ToTable("Pago");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.MontoTotal).HasColumnType("decimal(10,2)");

                entity.HasOne(p => p.MetodoPago)
                      .WithMany(m => m.Pagos)
                      .HasForeignKey(p => p.MetodoPagoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Usuario)
                      .WithMany(u => u.Pagos)
                      .HasForeignKey(p => p.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Cliente)
                      .WithMany(c => c.Pagos)
                      .HasForeignKey(p => p.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // PagoDetalle reparte un pago entre prestamos y contratos.
            // Las dos FK son opcionales: un detalle aplica a uno o a otro.
            modelBuilder.Entity<PagoDetalle>(entity =>
            {
                entity.ToTable("PagoDetalle");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Id).ValueGeneratedOnAdd();
                entity.Property(d => d.MontoAplicado).HasColumnType("decimal(10,2)");

                entity.HasOne(d => d.Pago)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(d => d.PagoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Prestamo)
                      .WithMany(p => p.PagoDetalles)
                      .HasForeignKey(d => d.PrestamoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Contrato)
                      .WithMany(c => c.PagoDetalles)
                      .HasForeignKey(d => d.ContratoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}