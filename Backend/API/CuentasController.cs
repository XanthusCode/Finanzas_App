using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[Route("[controller]")]
public class CuentasController(CuentaService service) : BaseController
{
    private readonly CuentaService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync(UserId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearCuentaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest(new { message = "El nombre es requerido." });

        var creada = await _service.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetAll), creada);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditarCuentaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest(new { message = "El nombre es requerido." });

        var result = await _service.UpdateAsync(id, dto, UserId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id, UserId);
        return ok ? NoContent() : NotFound();
    }

    // Historial unificado: movimientos manuales + ingresos y gastos de la cuenta.
    [HttpGet("{id:guid}/movimientos")]
    public async Task<IActionResult> GetMovimientos(Guid id)
    {
        var lineas = await _service.GetLedgerAsync(id, UserId);
        return lineas is null ? NotFound() : Ok(lineas);
    }

    [HttpPost("{id:guid}/movimientos")]
    public async Task<IActionResult> CrearMovimiento(Guid id, [FromBody] CrearMovimientoCuentaDto dto)
    {
        if (dto.Monto == 0) return BadRequest(new { message = "El monto del movimiento no puede ser 0." });

        var result = await _service.RegistrarMovimientoAsync(id, dto, UserId);
        return result is null ? NotFound() : Ok(result);
    }
}
