using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class BuscarController(BusquedaService service) : BaseController
{
    private readonly BusquedaService _service = service;

    // GET /buscar?q=netflix  |  /buscar?q=50.000
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] string? q)
    {
        var resultados = await _service.BuscarAsync(q, UserId);
        return Ok(resultados);
    }
}
