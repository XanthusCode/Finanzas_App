using Finanzas.Application.DTOs;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface ICuentaRepository
    {
        Task<IEnumerable<Cuenta>> GetAllAsync(Guid userId);
        Task<Cuenta?> GetByIdAsync(Guid id, Guid userId);
        Task<Cuenta> CreateAsync(Cuenta cuenta);
        Task<Cuenta> UpdateAsync(Cuenta cuenta);
        Task DeleteAsync(Guid id);

        Task<Dictionary<Guid, decimal>> GetSaldosAsync(Guid userId);
        Task<IEnumerable<MovimientoCuenta>> GetMovimientosAsync(Guid cuentaId);
        Task<IEnumerable<LineaCuentaDto>> GetLedgerAsync(Guid cuentaId, Guid userId);
        Task<MovimientoCuenta> AddMovimientoAsync(MovimientoCuenta movimiento);
        Task DeleteMovimientosByDeudaIdAsync(Guid deudaId);
    }
}
