using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class CategoriasController(CategoriasService service) : BaseController
{
    private readonly CategoriasService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _service.GetAllAsync(UserId);
        return Ok(categorias);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearCategoriaDto dto)
    {
        var creada = await _service.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetAll), creada);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CrearCategoriaDto dto)
    {
        var actualizada = await _service.UpdateAsync(id, dto, UserId);
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
