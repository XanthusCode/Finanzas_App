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

        public async Task<Dictionary<Guid, decimal>> GetMontosPagadosAsync(Guid userId)
        {
            var query =
                from a in _db.Abonos
                join d in _db.Deudas on a.DeudaId equals d.Id
                where d.UserId == userId
                group a by a.DeudaId into g
                select new { DeudaId = g.Key, Total = g.Sum(x => x.Monto) };

            return await query.ToDictionaryAsync(x => x.DeudaId, x => x.Total);
        }

        public async Task<IEnumerable<Abono>> GetAbonosAsync(Guid deudaId) =>
            await _db.Abonos
                .Where(a => a.DeudaId == deudaId)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

        public async Task<Abono> AddAbonoAsync(Abono abono)
        {
            _db.Abonos.Add(abono);
            await _db.SaveChangesAsync();
            return abono;
        }
    }
}
