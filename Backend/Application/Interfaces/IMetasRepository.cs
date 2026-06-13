using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IMetasRepository
    {
        Task<IEnumerable<Meta>> GetAllAsync(Guid userId);
        Task<Meta?> GetByIdAsync(Guid id, Guid userId);
        Task<Meta> CreateAsync(Meta meta);
        Task<Meta> UpdateAsync(Meta meta);
        Task DeleteAsync(Guid id);
    }
}
