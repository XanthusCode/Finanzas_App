using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class DeudasController(DeudaService service) : BaseController
{
    private readonly DeudaService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync(UserId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearDeudaDto dto)
    {
        var creada = await _service.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetAll), creada);
    }

    [HttpPatch("{id:guid}/pagar")]
    public async Task<IActionResult> TogglePagada(Guid id)
    {
        var result = await _service.MarcarPagadaAsync(id, UserId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, UserId);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("{id:guid}/abonos")]
    public async Task<IActionResult> GetAbonos(Guid id)
    {
        var abonos = await _service.GetAbonosAsync(id, UserId);
        return abonos is null ? NotFound() : Ok(abonos);
    }

    [HttpPost("{id:guid}/abonos")]
    public async Task<IActionResult> CrearAbono(Guid id, [FromBody] CrearAbonoDto dto)
    {
        if (dto.Monto <= 0) return BadRequest(new { message = "El monto del abono debe ser mayor a 0." });

        var result = await _service.RegistrarAbonoAsync(id, dto, UserId);
        return result is null ? NotFound() : Ok(result);
    }
}
