using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IIngresosRepository
    {
        Task<IEnumerable<Ingreso>> GetByMesAsync(int mes, int anio, Guid userId);
        Task<IEnumerable<Ingreso>> GetByAnioAsync(int anio, Guid userId);
        Task<Ingreso?> GetByIdAsync(Guid id, Guid userId);
        Task<Ingreso> CreateAsync(Ingreso ingreso);
        Task DeleteAsync(Guid id);
    }
}
