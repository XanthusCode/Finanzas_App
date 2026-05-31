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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Gasto>(e =>
            {
                e.HasKey(g => g.Id);
                e.Property(g => g.Categoria).IsRequired().HasMaxLength(100);
                e.Property(g => g.Detalle).IsRequired().HasMaxLength(150);
                e.Property(g => g.Monto).HasColumnType("decimal(12,2)");
                e.Property(g => g.Tipo).HasConversion<string>();
            });

            modelBuilder.Entity<Ingreso>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.Concepto).IsRequired().HasMaxLength(100);
                e.Property(i => i.Monto).HasColumnType("decimal(12,2)");
            });

            modelBuilder.Entity<Categoria>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
                e.Property(c => c.Tipo).HasConversion<string>();
            });
        }
    }
}
