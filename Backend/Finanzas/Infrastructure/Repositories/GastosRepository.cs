using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class GastosRepository(AppDbContext db) : IGastosRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Gasto>> GetByMesAsync(int mes, int anio) =>
            await _db.Gastos
                .Where(g => g.Mes == mes && g.Anio == anio)
                .OrderBy(g => g.Tipo)
                .ThenBy(g => g.Categoria)
                .ToListAsync();

        public async Task<Gasto?> GetByIdAsync(int id) =>
            await _db.Gastos.FindAsync(id);

        public async Task<Gasto> CreateAsync(Gasto gasto)
        {
            _db.Gastos.Add(gasto);
            await _db.SaveChangesAsync();
            return gasto;
        }

        public async Task<Gasto> UpdateAsync(Gasto gasto)
        {
            _db.Gastos.Update(gasto);
            await _db.SaveChangesAsync();
            return gasto;
        }

        public async Task DeleteAsync(int id)
        {
            var gasto = await _db.Gastos.FindAsync(id);
            if (gasto is not null)
            {
                _db.Gastos.Remove(gasto);
                await _db.SaveChangesAsync();
            }
        }
    }
}
