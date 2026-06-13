using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IPresupuestosRepository
    {
        Task<IEnumerable<Presupuesto>> GetAllAsync(Guid userId);
        Task<Presupuesto?> GetByIdAsync(Guid id, Guid userId);
        Task<Presupuesto?> GetByCategoriaAsync(string categoria, Guid userId);
        Task<Presupuesto> CreateAsync(Presupuesto presupuesto);
        Task<Presupuesto> UpdateAsync(Presupuesto presupuesto);
        Task DeleteAsync(Guid id);
    }
}
