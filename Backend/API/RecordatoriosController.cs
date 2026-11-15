using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Finanzas.API;

// Sin [Authorize]: lo dispara un cron externo (GitHub Actions), no un usuario logueado.
// Se protege con un secreto compartido en el header, no con JWT.
[Route("[controller]")]
[ApiController]
public class RecordatoriosController(RecordatoriosService service, AutomatizacionService automatizacionService, IConfiguration config) : ControllerBase
{
    private readonly RecordatoriosService _service = service;
    private readonly IConfiguration _config = config;

    [HttpPost("procesar")]
    public async Task<IActionResult> Procesar([FromHeader(Name = "X-Cron-Secret")] string? secret)
    {
        var esperado = _config["Recordatorios:CronSecret"];
        // Comparación en tiempo constante: con != el tiempo de respuesta revela cuántos
        // caracteres iniciales coinciden y el secreto se podría adivinar por partes.
        if (string.IsNullOrWhiteSpace(esperado) || esperado.StartsWith("CHANGE_ME") || secret is null
            || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(esperado)))
            return Unauthorized();

        await automatizacionService.ProcesarAsync();
        var enviados = await _service.ProcesarAsync();
        return Ok(new { enviados });
    }
}
