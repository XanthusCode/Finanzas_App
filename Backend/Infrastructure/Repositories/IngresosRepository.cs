using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class IngresosRepository(AppDbContext db) : IIngresosRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<IEnumerable<Ingreso>> GetByMesAsync(int mes, int anio, Guid userId) =>
            await _db.Ingresos
                .Where(i => i.Mes == mes && i.Anio == anio && i.UserId == userId)
                .OrderBy(i => i.Concepto)
                .ToListAsync();

        public async Task<IEnumerable<Ingreso>> GetByAnioAsync(int anio, Guid userId) =>
            await _db.Ingresos
                .Where(i => i.Anio == anio && i.UserId == userId)
                .ToListAsync();

        public async Task<IEnumerable<Ingreso>> GetRecurrentesAsync(int mes, int anio, Guid userId) =>
            await _db.Ingresos
                .Where(i => i.Mes == mes && i.Anio == anio && i.UserId == userId && i.EsRecurrente)
                .ToListAsync();

        public async Task<Ingreso?> GetByIdAsync(Guid id, Guid userId) =>
            await _db.Ingresos.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        public async Task<Ingreso> CreateAsync(Ingreso ingreso)
        {
            _db.Ingresos.Add(ingreso);
            await _db.SaveChangesAsync();
            return ingreso;
        }

        public async Task<Ingreso> UpdateAsync(Ingreso ingreso)
        {
            _db.Ingresos.Update(ingreso);
            await _db.SaveChangesAsync();
            return ingreso;
        }

        public async Task DeleteAsync(Guid id)
        {
            var ingreso = await _db.Ingresos.FindAsync(id);
            if (ingreso is not null)
            {
                _db.Ingresos.Remove(ingreso);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteByDeudaIdAsync(Guid deudaId)
        {
            var ingresos = await _db.Ingresos.Where(i => i.DeudaId == deudaId).ToListAsync();
            if (ingresos.Count > 0)
            {
                _db.Ingresos.RemoveRange(ingresos);
                await _db.SaveChangesAsync();
            }
        }

        // Búsqueda global: texto en el concepto o monto exacto, hasta el mes actual.
        public async Task<IEnumerable<Ingreso>> BuscarAsync(string? texto, decimal? monto, Guid userId, int limite)
        {
            var hoy    = DateTime.UtcNow;
            var patron = texto is null ? null : $"%{GastosRepository.EscaparLike(texto)}%";

            return await _db.Ingresos
                .Where(i => i.UserId == userId
                            && (i.Anio < hoy.Year || (i.Anio == hoy.Year && i.Mes <= hoy.Month))
                            && ((patron != null && EF.Functions.ILike(i.Concepto, patron, "\\"))
                                || (monto != null && i.Monto == monto)))
                .OrderByDescending(i => i.Anio)
                .ThenByDescending(i => i.Mes)
                .ThenByDescending(i => i.CreadoEn)
                .Take(limite)
                .ToListAsync();
        }
    }
}
