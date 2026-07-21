using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class CuentaRepository(AppDbContext db) : ICuentaRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Cuenta>> GetAllAsync(Guid userId) =>
            await _db.Cuentas
                .Where(c => c.UserId == userId && c.Activa)
                .OrderBy(c => c.CreadoEn)
                .ToListAsync();

        public async Task<Cuenta?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Cuentas.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        public async Task<Cuenta> CreateAsync(Cuenta cuenta)
        {
            _db.Cuentas.Add(cuenta);
            await _db.SaveChangesAsync();
            return cuenta;
        }

        public async Task<Cuenta> UpdateAsync(Cuenta cuenta)
        {
            _db.Cuentas.Update(cuenta);
            await _db.SaveChangesAsync();
            return cuenta;
        }

        public async Task DeleteAsync(Guid id)
        {
            var c = await _db.Cuentas.FindAsync(id);
            if (c is not null) { c.Activa = false; await _db.SaveChangesAsync(); }
        }

        public async Task<Dictionary<Guid, decimal>> GetSaldosAsync(Guid userId)
        {
            var movimientosQuery =
                from m in _db.MovimientosCuenta
                join c in _db.Cuentas on m.CuentaId equals c.Id
                where c.UserId == userId
                group m.Monto by m.CuentaId into g
                select new { CuentaId = g.Key, Total = g.Sum() };

            var ingresosQuery =
                from i in _db.Ingresos
                where i.UserId == userId && i.CuentaId != null
                group i.Monto by i.CuentaId!.Value into g
                select new { CuentaId = g.Key, Total = g.Sum() };

            var gastosQuery =
                from g in _db.Gastos
                where g.UserId == userId && g.CuentaId != null
                group g.Monto by g.CuentaId!.Value into grp
                select new { CuentaId = grp.Key, Total = -grp.Sum() };

            var saldos = new Dictionary<Guid, decimal>();

            foreach (var item in await movimientosQuery.ToListAsync())
                saldos[item.CuentaId] = saldos.GetValueOrDefault(item.CuentaId) + item.Total;

            foreach (var item in await ingresosQuery.ToListAsync())
                saldos[item.CuentaId] = saldos.GetValueOrDefault(item.CuentaId) + item.Total;

            foreach (var item in await gastosQuery.ToListAsync())
                saldos[item.CuentaId] = saldos.GetValueOrDefault(item.CuentaId) + item.Total;

            return saldos;
        }

        public async Task<IEnumerable<MovimientoCuenta>> GetMovimientosAsync(Guid cuentaId) =>
            await _db.MovimientosCuenta
                .Where(m => m.CuentaId == cuentaId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

        public async Task<MovimientoCuenta> AddMovimientoAsync(MovimientoCuenta movimiento)
        {
            _db.MovimientosCuenta.Add(movimiento);
            await _db.SaveChangesAsync();
            return movimiento;
        }
    }
}
