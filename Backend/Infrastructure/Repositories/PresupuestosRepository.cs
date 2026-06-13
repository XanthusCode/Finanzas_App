using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class PresupuestosRepository(AppDbContext db) : IPresupuestosRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Presupuesto>> GetAllAsync(Guid userId) =>
            await _db.Presupuestos.Where(p => p.UserId == userId).ToListAsync();

        public async Task<Presupuesto?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Presupuestos.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        public async Task<Presupuesto?> GetByCategoriaAsync(string categoria, Guid userId) =>
            await _db.Presupuestos.FirstOrDefaultAsync(p => p.Categoria == categoria && p.UserId == userId);

        public async Task<Presupuesto> CreateAsync(Presupuesto presupuesto)
        {
            _db.Presupuestos.Add(presupuesto);
            await _db.SaveChangesAsync();
            return presupuesto;
        }

        public async Task<Presupuesto> UpdateAsync(Presupuesto presupuesto)
        {
            _db.Presupuestos.Update(presupuesto);
            await _db.SaveChangesAsync();
            return presupuesto;
        }

        public async Task DeleteAsync(Guid id)
        {
            var p = await _db.Presupuestos.FindAsync(id);
            if (p is not null)
            {
                _db.Presupuestos.Remove(p);
                await _db.SaveChangesAsync();
            }
        }
    }
}
