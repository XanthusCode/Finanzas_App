using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IGastosRepository
    {
        Task<IEnumerable<Gasto>> GetByMesAsync(int mes, int anio);
        Task<Gasto?> GetByIdAsync(int id);
        Task<Gasto> CreateAsync(Gasto gasto);
        Task<Gasto> UpdateAsync(Gasto gasto);
        Task DeleteAsync(int id);
    }
}
