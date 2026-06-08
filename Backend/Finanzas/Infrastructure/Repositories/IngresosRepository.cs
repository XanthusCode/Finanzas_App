using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class IngresosRepository(AppDbContext db) : IIngresosRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Ingreso>> GetByMesAsync(int mes, int anio, Guid userId) =>
            await _db.Ingresos
                .Where(i => i.Mes == mes && i.Anio == anio && i.UserId == userId)
                .OrderBy(i => i.Concepto)
                .ToListAsync();

        public async Task<IEnumerable<Ingreso>> GetByAnioAsync(int anio, Guid userId) =>
            await _db.Ingresos
                .Where(i => i.Anio == anio && i.UserId == userId)
                .ToListAsync();

        public async Task<Ingreso?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Ingresos.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        public async Task<Ingreso> CreateAsync(Ingreso ingreso)
        {
            _db.Ingresos.Add(ingreso);
            await _db.SaveChangesAsync();
            return ingreso;
        }

        public async Task<Ingreso> UpdateAsync(Ingreso ingreso)
        {
            _db.Ingresos.Update(ingreso);
            await _db.SaveChangesAsync();
            return ingreso;
        }

        public async Task DeleteAsync(Guid id)
        {
            var ingreso = await _db.Ingresos.FindAsync(id);
            if (ingreso is not null)
            {
                _db.Ingresos.Remove(ingreso);
                await _db.SaveChangesAsync();
            }
        }
    }
}
