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

    [HttpPost("{id:guid}/abonar")]
    public async Task<IActionResult> Abonar(Guid id, [FromBody] AbonarMetaDto dto)
    {
        var actualizada = await _service.AbonarAsync(id, dto, UserId);
        if (actualizada is null) return NotFound();
        return Ok(actualizada);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var eliminada = await _service.DeleteAsync(id, UserId);
        if (!eliminada) return NotFound();
        return NoContent();
    }
}
