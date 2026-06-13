using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class GastosRepository(AppDbContext db) : IGastosRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Gasto>> GetByMesAsync(int mes, int anio, Guid userId) =>
            await _db.Gastos
                .Where(g => g.Mes == mes && g.Anio == anio && g.UserId == userId)
                .OrderBy(g => g.Tipo)
                .ThenBy(g => g.Categoria)
                .ToListAsync();

        public async Task<IEnumerable<Gasto>> GetByAnioAsync(int anio, Guid userId) =>
            await _db.Gastos
                .Where(g => g.Anio == anio && g.UserId == userId)
                .ToListAsync();

        public async Task<IEnumerable<Gasto>> GetRecurrentesAsync(int mes, int anio, Guid userId) =>
            await _db.Gastos
                .Where(g => g.Mes == mes && g.Anio == anio && g.UserId == userId && g.EsRecurrente)
                .ToListAsync();

        public async Task<Gasto?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Gastos.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

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

        public async Task DeleteAsync(Guid id)
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
