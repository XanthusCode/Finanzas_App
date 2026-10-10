using System.Collections.Generic;
using System.Reflection.Emit;
using Finanzas.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Finanzas.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<Meta> Metas { get; set; }
        public DbSet<MovimientoMeta> MovimientosMeta { get; set; }
        public DbSet<Deuda> Deudas { get; set; }
        public DbSet<Abono> Abonos { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<MovimientoCuenta> MovimientosCuenta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Gasto>(e =>
            {
                e.HasKey(g => g.Id);
                e.Property(g => g.Categoria).IsRequired().HasMaxLength(100);
                e.Property(g => g.Detalle).IsRequired().HasMaxLength(150);
                e.Property(g => g.Monto).HasColumnType("decimal(12,2)");
                e.Property(g => g.Tipo).HasConversion<string>();
                e.HasOne<User>().WithMany().HasForeignKey(g => g.UserId).OnDelete(DeleteBehavior.Cascade);
                e.Property(g => g.NumCuotas).IsRequired(false);
                e.Property(g => g.CuotaActual).IsRequired(false);
                e.Property(g => g.GastoOrigenId).IsRequired(false);
                e.Property(g => g.CuentaId).IsRequired(false);
                e.HasOne<Cuenta>().WithMany().HasForeignKey(g => g.CuentaId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Ingreso>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.Concepto).IsRequired().HasMaxLength(100);
                e.Property(i => i.Monto).HasColumnType("decimal(12,2)");
                e.HasOne<User>().WithMany().HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
                e.Property(i => i.CuentaId).IsRequired(false);
                e.HasOne<Cuenta>().WithMany().HasForeignKey(i => i.CuentaId).OnDelete(DeleteBehavior.SetNull);
                e.Property(i => i.Frecuencia).HasConversion<string>().IsRequired(false);
                e.Property(i => i.DiaPago).IsRequired(false);
            });

            modelBuilder.Entity<Categoria>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
                e.HasOne<User>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).IsRequired().HasMaxLength(200);
                e.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.WhatsAppId).HasMaxLength(64);
                e.Property(u => u.WhatsAppCodigo).HasMaxLength(6);
                e.HasIndex(u => u.WhatsAppId).IsUnique().HasFilter("\"WhatsAppId\" IS NOT NULL");
            });

            modelBuilder.Entity<Presupuesto>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Categoria).IsRequired().HasMaxLength(100);
                e.Property(p => p.Limite).HasColumnType("decimal(12,2)");
                e.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(p => new { p.Categoria, p.UserId }).IsUnique();
            });

            modelBuilder.Entity<Meta>(e =>
            {
                e.HasKey(m => m.Id);
                e.Property(m => m.Nombre).IsRequired().HasMaxLength(150);
                e.Property(m => m.MontoObjetivo).HasColumnType("decimal(12,2)");
                e.Property(m => m.MontoActual).HasColumnType("decimal(12,2)");
                e.HasOne<User>().WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
                e.Property(m => m.CuentaId).IsRequired(false);
                e.HasOne<Cuenta>().WithMany().HasForeignKey(m => m.CuentaId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<MovimientoMeta>(e =>
            {
                e.HasKey(m => m.Id);
                e.Property(m => m.Monto).HasColumnType("decimal(12,2)");
                e.Property(m => m.Descripcion).HasMaxLength(200);
                e.HasOne<Meta>().WithMany().HasForeignKey(m => m.MetaId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Deuda>(e =>
            {
                e.HasKey(d => d.Id);
                e.Property(d => d.Persona).IsRequired().HasMaxLength(100);
                e.Property(d => d.Descripcion).IsRequired().HasMaxLength(200);
                e.Property(d => d.Monto).HasColumnType("decimal(12,2)");
                e.Property(d => d.Tipo).HasConversion<string>();
                e.HasOne<User>().WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
                e.Property(d => d.CuentaId).IsRequired(false);
                e.HasOne<Cuenta>().WithMany().HasForeignKey(d => d.CuentaId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Abono>(e =>
            {
                e.HasKey(a => a.Id);
                e.Property(a => a.Monto).HasColumnType("decimal(12,2)");
                e.HasOne<Deuda>().WithMany().HasForeignKey(a => a.DeudaId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Cuenta>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
                e.Property(c => c.Tipo).HasConversion<string>();
                e.HasOne<User>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MovimientoCuenta>(e =>
            {
                e.HasKey(m => m.Id);
                e.Property(m => m.Monto).HasColumnType("decimal(12,2)");
                e.Property(m => m.Descripcion).HasMaxLength(150);
                e.Property(m => m.DeudaId).IsRequired(false);
                e.HasOne<Cuenta>().WithMany().HasForeignKey(m => m.CuentaId).OnDelete(DeleteBehavior.Cascade);
            });        }

        public override int SaveChanges()
        {
            SellarAuditoria();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SellarAuditoria();
            return base.SaveChangesAsync(cancellationToken);
        }

        // Sella ActualizadoEn en cada entidad IAuditable modificada.
        private void SellarAuditoria()
        {
            foreach (var entry in ChangeTracker.Entries<IAuditable>())
                if (entry.State == EntityState.Modified)
                    entry.Entity.ActualizadoEn = DateTime.UtcNow;
        }
    }
}
