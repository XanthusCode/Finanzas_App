using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IDeudaRepository
    {
        Task<IEnumerable<Deuda>> GetAllAsync(Guid userId);
        Task<Deuda?> GetByIdAsync(Guid id, Guid userId);
        Task<Deuda> CreateAsync(Deuda deuda);
        Task<Deuda> UpdateAsync(Deuda deuda);
        Task DeleteAsync(Guid id);
    }
}
