using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[ApiController]
[Route("[controller]")]
public class GastosController(GastosService service) : ControllerBase
{
    private readonly GastosService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int mes, [FromQuery] int anio)
    {
        var gastos = await _service.GetByMesAsync(mes, anio);
        return Ok(gastos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearGastoDto dto)
    {
        var creado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), creado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CrearGastoDto dto)
    {
        var actualizado = await _service.UpdateAsync(id, dto);
        if (actualizado is null) return NotFound();
        return Ok(actualizado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _service.DeleteAsync(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}