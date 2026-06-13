using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class PresupuestosController(PresupuestosService service) : BaseController
{
    private readonly PresupuestosService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync(UserId);
        return Ok(items);
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] CrearPresupuestoDto dto)
    {
        var result = await _service.UpsertAsync(dto, UserId);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var eliminado = await _service.DeleteAsync(id, UserId);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}
