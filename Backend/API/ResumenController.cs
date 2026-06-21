using Finanzas.Application.Interfaces;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class ResumenController(
    ResumenService service,
    ReporteService reporteService,
    IGastosRepository gastosRepo,
    IIngresosRepository ingresosRepo) : BaseController
{
    private readonly ResumenService    _service       = service;
    private readonly ReporteService    _reporteService = reporteService;
    private readonly IGastosRepository  _gastosRepo    = gastosRepo;
    private readonly IIngresosRepository _ingresosRepo  = ingresosRepo;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int mes, [FromQuery] int anio)
    {
        var resumen = await _service.GetAsync(mes, anio, UserId);
        return Ok(resumen);
    }

    [HttpGet("anual")]
    public async Task<IActionResult> GetAnual([FromQuery] int anio)
    {
        var resumen = await _service.GetAnualAsync(anio, UserId);
        return Ok(resumen);
    }

    [HttpGet("tendencia")]
    public async Task<IActionResult> GetTendencia([FromQuery] int anio)
    {
        var tendencia = await _service.GetTendenciaAsync(anio, UserId);
        return Ok(tendencia);
    }

    [HttpGet("gastos-por-categoria")]
    public async Task<IActionResult> GetGastosPorCategoria([FromQuery] int anio)
    {
        var data = await _service.GetGastosPorCategoriaAsync(anio, UserId);
        return Ok(data);
    }

    [HttpGet("reporte")]
    public async Task<IActionResult> GetReporte([FromQuery] int mes, [FromQuery] int anio)
    {
        var resumen  = await _service.GetAsync(mes, anio, UserId);
        var gastos   = await _gastosRepo.GetByMesAsync(mes, anio, UserId);
        var ingresos = await _ingresosRepo.GetByMesAsync(mes, anio, UserId);

        var pdf = _reporteService.Generar(resumen, gastos, ingresos);

        var nombreMes = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
            "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
        var filename = $"reporte-{(mes >= 1 && mes <= 12 ? nombreMes[mes] : mes.ToString()).ToLower()}-{anio}.pdf";

        return File(pdf, "application/pdf", filename);
    }
}
