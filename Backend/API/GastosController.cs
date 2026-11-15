using Microsoft.AspNetCore.RateLimiting;
using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class GastosController(GastosService service) : BaseController
{
    private readonly GastosService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int mes, [FromQuery] int anio)
    {
        var gastos = await _service.GetByMesAsync(mes, anio, UserId);
        return Ok(gastos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearGastoDto dto)
    {
        if (!PeriodoValido(dto.Mes, dto.Anio)) return BadRequest("Mes o año inválido.");
        var creado = await _service.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetAll), creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CrearGastoDto dto)
    {
        if (!PeriodoValido(dto.Mes, dto.Anio)) return BadRequest("Mes o año inválido.");
        var actualizado = await _service.UpdateAsync(id, dto, UserId);
        if (actualizado is null) return NotFound();
        return Ok(actualizado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var eliminado = await _service.DeleteAsync(id, UserId);
        if (!eliminado) return NotFound();
        return NoContent();
    }

    [HttpPost("copiar-recurrentes")]
    public async Task<IActionResult> CopiarRecurrentes([FromQuery] int mes, [FromQuery] int anio)
    {
        if (!PeriodoValido(mes, anio)) return BadRequest("Mes o año inválido.");
        var copiados = await _service.CopiarRecurrentesAsync(mes, anio, UserId);
        return Ok(copiados);
    }

    [HttpPost("importar")]
    public async Task<IActionResult> Importar([FromQuery] int mes, [FromQuery] int anio, [FromBody] IEnumerable<CrearGastoDto> dtos)
    {
        if (!PeriodoValido(mes, anio)) return BadRequest("Mes o año inválido.");
        var count = await _service.ImportarAsync(dtos, mes, anio, UserId);
        return Ok(new { importados = count });
    }

    // Mismos formatos que acepta el frontend; el archivo va directo a la IA, así que se acota
    private static readonly string[] TiposRecibo = ["image/jpeg", "image/png", "application/pdf"];
    private const long MaxRecibo = 5 * 1024 * 1024;

    [EnableRateLimiting("ia")]
    [HttpPost("importar-recibo")]
    [RequestSizeLimit(MaxRecibo + 64 * 1024)] // + margen para las cabeceras del multipart
    public async Task<IActionResult> ImportarRecibo(IFormFile? recibo)
    {
        if (recibo is null || recibo.Length == 0)
            return BadRequest(new { message = "Adjunta una foto o PDF del recibo." });
        if (recibo.Length > MaxRecibo)
            return BadRequest(new { message = "El archivo pesa más de 5 MB." });
        if (!TiposRecibo.Contains(recibo.ContentType))
            return BadRequest(new { message = "Formato no soportado. Usa JPG, PNG o PDF." });

        var extraido = await _service.EscanearReciboAsync(recibo);
        return Ok(extraido);
    }

    [HttpGet("resumen-categoria")]
    public async Task<IActionResult> GetResumenCategoria([FromQuery] int mes, [FromQuery] int anio)
    {
        var resumen = await _service.GetResumenCategoriaAsync(mes, anio, UserId);
        return Ok(resumen);
    }

    [HttpGet("cuotas")]
    public async Task<IActionResult> GetCuotas()
    {
        var cuotas = await _service.GetCuotasAsync(UserId);
        return Ok(cuotas);
    }

    [HttpGet("suscripciones")]
    public async Task<IActionResult> GetSuscripciones()
    {
        var suscripciones = await _service.GetSuscripcionesAsync(UserId);
        return Ok(suscripciones);
    }

    [HttpGet("heatmap")]
    public async Task<IActionResult> GetHeatmap([FromQuery] int anio)
    {
        var heatmap = await _service.GetHeatmapAsync(anio, UserId);
        return Ok(heatmap);
    }
}
