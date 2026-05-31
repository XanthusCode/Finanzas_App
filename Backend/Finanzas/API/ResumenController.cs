using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[ApiController]
[Route("[controller]")]
public class ResumenController(ResumenService service) : ControllerBase
{
    private readonly ResumenService _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int mes, [FromQuery] int anio)
    {
        var resumen = await _service.GetAsync(mes, anio);
        return Ok(resumen);
    }
}