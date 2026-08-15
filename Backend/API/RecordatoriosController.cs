using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

// Sin [Authorize]: lo dispara un cron externo (GitHub Actions), no un usuario logueado.
// Se protege con un secreto compartido en el header, no con JWT.
[Route("[controller]")]
[ApiController]
public class RecordatoriosController(RecordatoriosService service, IConfiguration config) : ControllerBase
{
    private readonly RecordatoriosService _service = service;
    private readonly IConfiguration _config = config;

    [HttpPost("procesar")]
    public async Task<IActionResult> Procesar([FromHeader(Name = "X-Cron-Secret")] string? secret)
    {
        var esperado = _config["Recordatorios:CronSecret"];
        if (string.IsNullOrWhiteSpace(esperado) || secret != esperado)
            return Unauthorized();

        var enviados = await _service.ProcesarAsync();
        return Ok(new { enviados });
    }
}
