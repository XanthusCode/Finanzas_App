using Finanzas.Application.DTOs;
using Microsoft.AspNetCore.RateLimiting;
using Finanzas.Application.DTOs.IA;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class CopilotoController(CopilotoService service) : BaseController
{
    private readonly CopilotoService _service = service;

    [EnableRateLimiting("ia")]
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto dto)
    {
        var respuesta = await _service.ResponderAsync(dto.Mensajes, UserId);
        if (respuesta is null) return Ok(new { estado = "error" });
        return Ok(new { estado = "ok", respuesta });
    }

}
