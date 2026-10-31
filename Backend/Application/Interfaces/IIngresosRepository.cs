using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IIngresosRepository
    {
        Task<IEnumerable<Ingreso>> GetByMesAsync(int mes, int anio, Guid userId);
        Task<IEnumerable<Ingreso>> GetByAnioAsync(int anio, Guid userId);
        Task<IEnumerable<Ingreso>> GetRecurrentesAsync(int mes, int anio, Guid userId);
        Task<Ingreso?> GetByIdAsync(Guid id, Guid userId);
        Task<Ingreso> CreateAsync(Ingreso ingreso);
        Task<Ingreso> UpdateAsync(Ingreso ingreso);
        Task DeleteAsync(Guid id);
        Task DeleteByDeudaIdAsync(Guid deudaId);
        /// <summary>Ingresos hasta el mes actual cuyo concepto contiene <paramref name="texto"/> o cuyo monto es <paramref name="monto"/>.</summary>
        Task<IEnumerable<Ingreso>> BuscarAsync(string? texto, decimal? monto, Guid userId, int limite);
    }
}
