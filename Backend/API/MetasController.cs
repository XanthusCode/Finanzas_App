using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class MetasController(MetasService service) : BaseController
{
    private readonly MetasService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync(UserId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearMetaDto dto)
    {
        var creada = await _service.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetAll), creada);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarMetaDto dto)
    {
        var actualizada = await _service.EditarAsync(id, dto, UserId);
        if (actualizada is null) return NotFound();
        return Ok(actualizada);
    }

    [HttpGet("{id:guid}/historial")]
    public async Task<IActionResult> GetMovimientos(Guid id)
    {
        var movimientos = await _service.GetMovimientosAsync(id, UserId);
        return movimientos is null ? NotFound() : Ok(movimientos);
    }

    [HttpPost("{id:guid}/movimientos")]
    public async Task<IActionResult> CrearMovimiento(Guid id, [FromBody] CrearMovimientoMetaDto dto)
    {
        if (dto.Monto == 0) return BadRequest(new { message = "El monto del movimiento no puede ser 0." });

        try
        {
            var result = await _service.RegistrarMovimientoAsync(id, dto, UserId);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var eliminada = await _service.DeleteAsync(id, UserId);
        if (!eliminada) return NotFound();
        return NoContent();
    }
}
