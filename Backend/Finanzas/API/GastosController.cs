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
        var creado = await _service.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetAll), creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CrearGastoDto dto)
    {
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
}
