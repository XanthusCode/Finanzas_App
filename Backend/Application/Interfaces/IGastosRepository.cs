using Finanzas.Application.DTOs;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IGastosRepository
    {
        Task<IEnumerable<Gasto>> GetByMesAsync(int mes, int anio, Guid userId);
        Task<IEnumerable<Gasto>> GetByAnioAsync(int anio, Guid userId);
        Task<IEnumerable<Gasto>> GetRecurrentesAsync(int mes, int anio, Guid userId);
        Task<bool> ExistePorCategoriaAsync(string nombreCategoria, Guid userId);
        Task<IEnumerable<Gasto>> GetByCategoriaAsync(string nombreCategoria, Guid userId);
        Task<Gasto?> GetByIdAsync(Guid id, Guid userId);
        Task<Gasto> CreateAsync(Gasto gasto);
        Task<Gasto> UpdateAsync(Gasto gasto);
        Task DeleteAsync(Guid id);
        Task DeleteByDeudaIdAsync(Guid deudaId);
        Task<IEnumerable<ResumenCategoriaDto>> GetResumenCategoriaAsync(int mes, int anio, Guid userId);
        Task<IEnumerable<Gasto>> GetCuotasAsync(Guid userId);
        Task<DateTime?> GetUltimaActividadAsync(Guid userId);
        /// <summary>Gastos hasta el mes actual cuyo detalle/categoría contiene <paramref name="texto"/> o cuyo monto es <paramref name="monto"/>.</summary>
        Task<IEnumerable<Gasto>> BuscarAsync(string? texto, decimal? monto, Guid userId, int limite);
    }
}
