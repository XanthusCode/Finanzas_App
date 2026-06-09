using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IGastosRepository
    {
        Task<IEnumerable<Gasto>> GetByMesAsync(int mes, int anio, Guid userId);
        Task<IEnumerable<Gasto>> GetByAnioAsync(int anio, Guid userId);
        Task<Gasto?> GetByIdAsync(Guid id, Guid userId);
        Task<Gasto> CreateAsync(Gasto gasto);
        Task<Gasto> UpdateAsync(Gasto gasto);
        Task DeleteAsync(Guid id);
    }
}
