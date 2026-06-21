using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class DeudaRepository(AppDbContext db) : IDeudaRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Deuda>> GetAllAsync(Guid userId) =>
            await _db.Deudas
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.Pagada)
                .ThenBy(d => d.FechaLimite)
                .ThenBy(d => d.CreadoEn)
                .ToListAsync();

        public async Task<Deuda?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Deudas.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

        public async Task<Deuda> CreateAsync(Deuda deuda)
        {
            _db.Deudas.Add(deuda);
            await _db.SaveChangesAsync();
            return deuda;
        }

        public async Task<Deuda> UpdateAsync(Deuda deuda)
        {
            _db.Deudas.Update(deuda);
            await _db.SaveChangesAsync();
            return deuda;
        }

        public async Task DeleteAsync(Guid id)
        {
            var d = await _db.Deudas.FindAsync(id);
            if (d is not null) { _db.Deudas.Remove(d); await _db.SaveChangesAsync(); }
        }
    }
}
