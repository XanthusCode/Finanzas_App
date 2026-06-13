using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class MetasRepository(AppDbContext db) : IMetasRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Meta>> GetAllAsync(Guid userId) =>
            await _db.Metas.Where(m => m.UserId == userId).OrderBy(m => m.CreadoEn).ToListAsync();

        public async Task<Meta?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Metas.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        public async Task<Meta> CreateAsync(Meta meta)
        {
            _db.Metas.Add(meta);
            await _db.SaveChangesAsync();
            return meta;
        }

        public async Task<Meta> UpdateAsync(Meta meta)
        {
            _db.Metas.Update(meta);
            await _db.SaveChangesAsync();
            return meta;
        }

        public async Task DeleteAsync(Guid id)
        {
            var m = await _db.Metas.FindAsync(id);
            if (m is not null)
            {
                _db.Metas.Remove(m);
                await _db.SaveChangesAsync();
            }
        }
    }
}
