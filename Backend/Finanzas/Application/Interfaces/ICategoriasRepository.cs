using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface ICategoriasRepository
    {
        Task<IEnumerable<Categoria>> GetAllAsync(Guid userId);
        Task<Categoria?> GetByIdAsync(Guid id, Guid userId);
        Task<Categoria> CreateAsync(Categoria categoria);
        Task<Categoria> UpdateAsync(Categoria categoria);
        Task DeleteAsync(Guid id);
    }
}
