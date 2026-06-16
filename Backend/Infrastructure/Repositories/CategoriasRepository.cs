using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class CategoriasRepository(AppDbContext db) : ICategoriasRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Categoria>> GetAllAsync(Guid userId) =>
            await _db.Categorias
                .Where(c => c.Activa && c.UserId == userId)
                .OrderBy(c => c.Tipo)
                .ThenBy(c => c.Nombre)
                .ToListAsync();

        public async Task<Categoria?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        public async Task<Categoria> CreateAsync(Categoria categoria)
        {
            _db.Categorias.Add(categoria);
            await _db.SaveChangesAsync();
            return categoria;
        }

        public async Task<Categoria> UpdateAsync(Categoria categoria)
        {
            _db.Categorias.Update(categoria);
            await _db.SaveChangesAsync();
            return categoria;
        }

        public async Task DeleteAsync(Guid id)
        {
            var categoria = await _db.Categorias.FindAsync(id);

            if (categoria is not null)
            {
                _db.Categorias.Remove(categoria);
                await _db.SaveChangesAsync();
            }
        }
    }
}
