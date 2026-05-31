using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services;

public class ResumenService
{
    private readonly IGastosRepository _gastosRepo;
    private readonly IIngresosRepository _ingresosRepo;

    public ResumenService(IGastosRepository gastosRepo, IIngresosRepository ingresosRepo)
    {
        _gastosRepo = gastosRepo;
        _ingresosRepo = ingresosRepo;
    }

    public async Task<ResumenDto> GetAsync(int mes, int anio)
    {
        var gastos = await _gastosRepo.GetByMesAsync(mes, anio);
        var ingresos = await _ingresosRepo.GetByMesAsync(mes, anio);

        var totalIngresos = ingresos.Sum(i => i.Monto);
        var totalFijos = gastos.Where(g => g.Tipo == TipoGasto.FIJO).Sum(g => g.Monto);
        var totalVariables = gastos.Where(g => g.Tipo == TipoGasto.VARIABLE).Sum(g => g.Monto);
        var totalGastos = totalFijos + totalVariables;
        var ahorro = totalIngresos - totalGastos;
        var pctAhorro = totalIngresos > 0 ? (int)Math.Round(ahorro / totalIngresos * 100) : 0;

        return new ResumenDto
        {
            Mes = mes,
            Anio = anio,
            TotalIngresos = totalIngresos,
            TotalFijos = totalFijos,
            TotalVariables = totalVariables,
            TotalGastos = totalGastos,
            Ahorro = ahorro,
            PctAhorro = pctAhorro,
        };
    }
}