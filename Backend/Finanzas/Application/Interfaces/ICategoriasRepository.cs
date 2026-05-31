using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface ICategoriasRepository
    {
        Task<IEnumerable<Categoria>> GetAllAsync();
        Task<Categoria?> GetByIdAsync(int id);
        Task<Categoria> CreateAsync(Categoria categoria);
        Task<Categoria> UpdateAsync(Categoria categoria);
        Task DeleteAsync(int id);
    }
}
