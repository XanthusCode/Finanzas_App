using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class CategoriasRepository(AppDbContext db) : ICategoriasRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Categoria>> GetAllAsync() =>
            await _db.Categorias
                .Where(c => c.Activa)
                .OrderBy(c => c.Tipo)
                .ThenBy(c => c.Nombre)
                .ToListAsync();

        public async Task<Categoria?> GetByIdAsync(int id) =>
            await _db.Categorias.FindAsync(id);

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

        public async Task DeleteAsync(int id)
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
