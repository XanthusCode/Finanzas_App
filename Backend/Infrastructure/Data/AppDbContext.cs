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
            });

            modelBuilder.Entity<Ingreso>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.Concepto).IsRequired().HasMaxLength(100);
                e.Property(i => i.Monto).HasColumnType("decimal(12,2)");
                e.HasOne<User>().WithMany().HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Categoria>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
                e.Property(c => c.Tipo).HasConversion<string>();
                e.HasOne<User>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).IsRequired().HasMaxLength(200);
                e.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
                e.Property(u => u.PasswordHash).IsRequired();
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
            });
        }
    }
}
