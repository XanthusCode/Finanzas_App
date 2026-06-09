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
}
