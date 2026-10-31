using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class GastosRepository(AppDbContext db) : IGastosRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Gasto>> GetByMesAsync(int mes, int anio, Guid userId) =>
            await _db.Gastos
                .Where(g => g.Mes == mes && g.Anio == anio && g.UserId == userId)
                .OrderBy(g => g.Tipo)
                .ThenBy(g => g.Categoria)
                .ToListAsync();

        public async Task<IEnumerable<Gasto>> GetByAnioAsync(int anio, Guid userId) =>
            await _db.Gastos
                .Where(g => g.Anio == anio && g.UserId == userId)
                .ToListAsync();

        public async Task<IEnumerable<Gasto>> GetRecurrentesAsync(int mes, int anio, Guid userId) =>
            await _db.Gastos
                .Where(g => g.Mes == mes && g.Anio == anio && g.UserId == userId && g.EsRecurrente)
                .ToListAsync();

        public async Task<bool> ExistePorCategoriaAsync(string nombreCategoria, Guid userId) =>
             await _db.Gastos.AnyAsync(g => g.Categoria == nombreCategoria && g.UserId == userId);

        // Solo hasta el mes actual: las cuotas de meses futuros aún no son movimientos reales.
        public async Task<IEnumerable<Gasto>> GetByCategoriaAsync(string nombreCategoria, Guid userId)
        {
            var hoy  = DateTime.UtcNow;
            var anio = hoy.Year;
            var mes  = hoy.Month;

            return await _db.Gastos
                .Where(g => g.Categoria == nombreCategoria && g.UserId == userId
                            && (g.Anio < anio || (g.Anio == anio && g.Mes <= mes)))
                .OrderByDescending(g => g.Anio)
                .ThenByDescending(g => g.Mes)
                .ThenByDescending(g => g.Fecha ?? g.CreadoEn)
                .ToListAsync();
        }

        public async Task<Gasto?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Gastos.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

        public async Task<Gasto> CreateAsync(Gasto gasto)
        {
            _db.Gastos.Add(gasto);
            await _db.SaveChangesAsync();
            return gasto;
        }

        public async Task<Gasto> UpdateAsync(Gasto gasto)
        {
            _db.Gastos.Update(gasto);
            await _db.SaveChangesAsync();
            return gasto;
        }

        public async Task DeleteAsync(Guid id)
        {
            var gasto = await _db.Gastos.FindAsync(id);
            if (gasto is not null)
            {
                _db.Gastos.Remove(gasto);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteByDeudaIdAsync(Guid deudaId)
        {
            var gastos = await _db.Gastos.Where(g => g.DeudaId == deudaId).ToListAsync();
            if (gastos.Count > 0)
            {
                _db.Gastos.RemoveRange(gastos);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ResumenCategoriaDto>> GetResumenCategoriaAsync(int mes, int anio, Guid userId) =>
            await _db.Gastos
                .Where(g => g.Mes == mes && g.Anio == anio && g.UserId == userId)
                .GroupBy(g => g.Categoria)
                .Select(grp => new ResumenCategoriaDto
                {
                    Categoria = grp.Key,
                    Total = grp.Sum(g => g.Monto)
                })
                .ToListAsync();

        public async Task<IEnumerable<Gasto>> GetCuotasAsync(Guid userId) =>
            await _db.Gastos
                .Where(g => g.UserId == userId && g.NumCuotas != null)
                .OrderBy(g => g.GastoOrigenId)
                .ThenBy(g => g.CuotaActual)
                .ToListAsync();

        // Último momento en que el usuario creó o editó un gasto (señal de que sigue usando la app).
        public async Task<DateTime?> GetUltimaActividadAsync(Guid userId) =>
            await _db.Gastos
                .Where(g => g.UserId == userId)
                .MaxAsync(g => (DateTime?)(g.ActualizadoEn ?? g.CreadoEn));

        // Búsqueda global: texto en detalle/categoría (sin distinguir mayúsculas) o monto exacto.
        // Solo hasta el mes actual, igual que GetByCategoriaAsync (las cuotas futuras aún no son reales).
        public async Task<IEnumerable<Gasto>> BuscarAsync(string? texto, decimal? monto, Guid userId, int limite)
        {
            var hoy    = DateTime.UtcNow;
            var patron = texto is null ? null : $"%{EscaparLike(texto)}%";

            return await _db.Gastos
                .Where(g => g.UserId == userId
                            && (g.Anio < hoy.Year || (g.Anio == hoy.Year && g.Mes <= hoy.Month))
                            && ((patron != null && (EF.Functions.ILike(g.Detalle, patron, "\\")
                                                    || EF.Functions.ILike(g.Categoria, patron, "\\")))
                                || (monto != null && g.Monto == monto)))
                .OrderByDescending(g => g.Anio)
                .ThenByDescending(g => g.Mes)
                .ThenByDescending(g => g.Fecha ?? g.CreadoEn)
                .Take(limite)
                .ToListAsync();
        }

        internal static string EscaparLike(string s) =>
            s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
    }
}
