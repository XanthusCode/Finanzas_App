using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[ApiController]
[Route("[controller]")]
public class IngresosController(IngresosService service) : ControllerBase
{
    private readonly IngresosService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int mes, [FromQuery] int anio)
    {
        var ingresos = await _service.GetByMesAsync(mes, anio);
        return Ok(ingresos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearIngresoDto dto)
    {
        var creado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), creado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _service.DeleteAsync(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}