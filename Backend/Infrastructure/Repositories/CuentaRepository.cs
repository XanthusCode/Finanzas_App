using Finanzas.Application.DTOs;
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
            // El saldo solo cuenta lo que YA ocurrió: las cuotas de meses futuros
            // (Gastos con Mes/Anio posterior al actual) no afectan el saldo todavía,
            // ni los gastos del mes actual que aún no tienen fecha de pago.
            var hoy   = DateTime.UtcNow;
            var anio  = hoy.Year;
            var mes   = hoy.Month;

            var movimientosQuery =
                from m in _db.MovimientosCuenta
                join c in _db.Cuentas on m.CuentaId equals c.Id
                where c.UserId == userId
                group m.Monto by m.CuentaId into g
                select new { CuentaId = g.Key, Total = g.Sum() };

            var ingresosQuery =
                from i in _db.Ingresos
                where i.UserId == userId && i.CuentaId != null
                      && (i.Anio < anio || (i.Anio == anio && i.Mes <= mes))
                group i.Monto by i.CuentaId!.Value into g
                select new { CuentaId = g.Key, Total = g.Sum() };

            // En el mes actual, un gasto sin Fecha (copia recurrente o cuota) se considera
            // pendiente de cobro: no descuenta hasta que se registre la fecha de pago.
            var gastosQuery =
                from g in _db.Gastos
                where g.UserId == userId && g.CuentaId != null
                      && (g.Anio < anio || (g.Anio == anio && g.Mes < mes)
                          || (g.Anio == anio && g.Mes == mes && g.Fecha != null && g.Fecha <= hoy))
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

        public async Task<IEnumerable<LineaCuentaDto>> GetLedgerAsync(Guid cuentaId, Guid userId)
        {
            // Igual que el saldo: no mostrar cuotas/ingresos de meses futuros.
            var hoy  = DateTime.UtcNow;
            var anio = hoy.Year;
            var mes  = hoy.Month;

            var movimientos = await _db.MovimientosCuenta
                .Where(m => m.CuentaId == cuentaId)
                .Select(m => new LineaCuentaDto
                {
                    Tipo        = "MOVIMIENTO",
                    Monto       = m.Monto,
                    Descripcion = m.Descripcion,
                    Fecha       = m.Fecha
                })
                .ToListAsync();

            var ingresos = await _db.Ingresos
                .Where(i => i.CuentaId == cuentaId && i.UserId == userId
                            && (i.Anio < anio || (i.Anio == anio && i.Mes <= mes)))
                .Select(i => new LineaCuentaDto
                {
                    Tipo        = "INGRESO",
                    Monto       = i.Monto,
                    Descripcion = i.Concepto,
                    Fecha       = i.CreadoEn
                })
                .ToListAsync();

            // Los gastos en cuotas guardan la fecha solo en la primera cuota; el resto
            // tiene Fecha null. Usar Mes/Anio como fecha para que cada cuota caiga en su
            // mes y no todas se amontonen en la fecha de creación.
            var gastosRaw = await _db.Gastos
                .Where(g => g.CuentaId == cuentaId && g.UserId == userId
                            && (g.Anio < anio || (g.Anio == anio && g.Mes < mes)
                                || (g.Anio == anio && g.Mes == mes && g.Fecha != null && g.Fecha <= hoy)))
                .Select(g => new { g.Monto, g.Detalle, g.Categoria, g.Fecha, g.Anio, g.Mes, g.CreadoEn })
                .ToListAsync();

            var gastos = gastosRaw.Select(g => new LineaCuentaDto
            {
                Tipo        = "GASTO",
                Monto       = -g.Monto,
                Descripcion = g.Detalle != "" ? g.Detalle : g.Categoria,
                Fecha       = g.Fecha ?? (g.Anio >= 1 && g.Mes is >= 1 and <= 12
                                  ? new DateTime(g.Anio, g.Mes, 1, 0, 0, 0, DateTimeKind.Utc)
                                  : g.CreadoEn)
            }).ToList();

            return movimientos
                .Concat(ingresos)
                .Concat(gastos)
                .OrderByDescending(l => l.Fecha)
                .ToList();
        }

        public async Task<MovimientoCuenta> AddMovimientoAsync(MovimientoCuenta movimiento)
        {
            _db.MovimientosCuenta.Add(movimiento);
            await _db.SaveChangesAsync();
            return movimiento;
        }

        public async Task DeleteMovimientosByDeudaIdAsync(Guid deudaId)
        {
            var movimientos = await _db.MovimientosCuenta
                .Where(m => m.DeudaId == deudaId)
                .ToListAsync();

            if (movimientos.Count > 0)
            {
                _db.MovimientosCuenta.RemoveRange(movimientos);
                await _db.SaveChangesAsync();
            }
        }
    }
}
