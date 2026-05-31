using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[ApiController]
[Route("[controller]")]
public class CategoriasController(CategoriasService service) : ControllerBase
{
    private readonly CategoriasService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _service.GetAllAsync();
        return Ok(categorias);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearCategoriaDto dto)
    {
        var creada = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), creada);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CrearCategoriaDto dto)
    {
        var actualizada = await _service.UpdateAsync(id, dto);
        if (actualizada is null) return NotFound();
        return Ok(actualizada);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminada = await _service.DeleteAsync(id);
        if (!eliminada) return NotFound();
        return NoContent();
    }
}