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

    // Cantidad de categorías activas del usuario
    [HttpGet("total")]
    public async Task<IActionResult> GetTotal() =>
        Ok(await _service.ContarAsync(UserId));

    // Cantidad de categorías con al menos un gasto registrado
    [HttpGet("con-gastos")]
    public async Task<IActionResult> GetTotalConGastos() =>
        Ok(await _service.ContarConGastosAsync(UserId));

    // Todos los gastos registrados en la categoría (montos en COP)
    [HttpGet("{id:guid}/gastos")]
    public async Task<IActionResult> GetGastos(Guid id)
    {
        var detalle = await _service.GetGastosAsync(id, UserId);
        if (detalle is null) return NotFound();
        return Ok(detalle);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearCategoriaDto dto)
    {
        var creada = await _service.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetAll), creada);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarCategoriaDto dto)
    {
        var actualizada = await _service.UpdateAsync(id, dto, UserId);
        if (actualizada is null) return NotFound();
        return Ok(actualizada);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var eliminada = await _service.DeleteAsync(id, UserId);
            if (!eliminada) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
