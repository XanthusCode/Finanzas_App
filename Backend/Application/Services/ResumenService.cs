using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services;

public class ResumenService(IGastosRepository gastosRepo, IIngresosRepository ingresosRepo)
{
    public async Task<ResumenDto> GetAnualAsync(int anio, Guid userId)
    {
        var gastos   = await gastosRepo.GetByAnioAsync(anio, userId);
        var ingresos = await ingresosRepo.GetByAnioAsync(anio, userId);

        // Solo meses donde ya hay ingresos registrados (excluye cuotas pre-generadas futuras)
        var mesesActivos  = ingresos.Select(i => i.Mes).ToHashSet();
        var gastosActivos = gastos.Where(g => mesesActivos.Contains(g.Mes));

        return Calcular(gastosActivos, ingresos, 0, anio);
    }

    public async Task<IEnumerable<ResumenDto>> GetTendenciaAsync(int anio, Guid userId)
    {
        var todosGastos   = await gastosRepo.GetByAnioAsync(anio, userId);
        var todosIngresos = await ingresosRepo.GetByAnioAsync(anio, userId);

        return Enumerable.Range(1, 12)
            .Select(mes => Calcular(
                todosGastos.Where(g => g.Mes == mes),
                todosIngresos.Where(i => i.Mes == mes),
                mes, anio))
            .ToList();
    }

    public async Task<ResumenDto> GetAsync(int mes, int anio, Guid userId)
    {
        var gastos   = await gastosRepo.GetByMesAsync(mes, anio, userId);
        var ingresos = await ingresosRepo.GetByMesAsync(mes, anio, userId);
        return Calcular(gastos, ingresos, mes, anio);
    }

    public async Task<IEnumerable<GastoCategoriaAnualDto>> GetGastosPorCategoriaAsync(int anio, Guid userId)
    {
        var gastos = await gastosRepo.GetByAnioAsync(anio, userId);

        return gastos
            .GroupBy(g => g.Categoria)
            .Select(grp => new GastoCategoriaAnualDto
            {
                Categoria = grp.Key,
                Datos = Enumerable.Range(1, 12)
                    .Select(mes => grp.Where(g => g.Mes == mes).Sum(g => g.Monto))
                    .ToArray()
            })
            .OrderByDescending(c => c.Datos.Sum())
            .Take(6)
            .ToList();
    }

    private static ResumenDto Calcular(IEnumerable<Gasto> gastos, IEnumerable<Ingreso> ingresos, int mes, int anio)
    {
        var totalIngresos  = ingresos.Sum(i => i.Monto);
        var totalFijos     = gastos.Where(g => g.Tipo == TipoGasto.FIJO).Sum(g => g.Monto);
        var totalVariables = gastos.Where(g => g.Tipo == TipoGasto.VARIABLE).Sum(g => g.Monto);
        var totalGastos    = totalFijos + totalVariables;
        var ahorro         = totalIngresos - totalGastos;
        var pctAhorro      = totalIngresos > 0 ? (int)Math.Round(ahorro / totalIngresos * 100) : 0;

        return new ResumenDto
        {
            Mes            = mes,
            Anio           = anio,
            TotalIngresos  = totalIngresos,
            TotalFijos     = totalFijos,
            TotalVariables = totalVariables,
            TotalGastos    = totalGastos,
            Ahorro         = ahorro,
            PctAhorro      = pctAhorro,
        };
    }
}
