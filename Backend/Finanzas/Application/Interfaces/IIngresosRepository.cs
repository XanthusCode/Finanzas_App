using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IIngresosRepository
    {
        Task<IEnumerable<Ingreso>> GetByMesAsync(int mes, int anio);
        Task<Ingreso?> GetByIdAsync(int id);
        Task<Ingreso> CreateAsync(Ingreso ingreso);
        Task DeleteAsync(int id);
    }
}
