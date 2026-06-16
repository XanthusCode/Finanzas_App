using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class IngresosController(IngresosService service) : BaseController
{
    private readonly IngresosService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int mes, [FromQuery] int anio)
    {
        var ingresos = await _service.GetByMesAsync(mes, anio, UserId);
        return Ok(ingresos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearIngresoDto dto)
    {
        var creado = await _service.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetAll), creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarIngresoDto dto)
    {
        var actualizada = await _service.UpdateAsync(id, dto, UserId);
        if (actualizada is null) return NotFound();
        return Ok(actualizada);
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
        var copiados = await _service.CopiarRecurrentesAsync(mes, anio, UserId);
        return Ok(copiados);
    }
}
