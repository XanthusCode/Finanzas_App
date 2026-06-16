using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class ResumenController(ResumenService service) : BaseController
{
    private readonly ResumenService _service = service;

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
}
