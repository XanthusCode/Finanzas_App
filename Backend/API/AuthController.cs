using System.Security.Claims;
using Finanzas.Application.DTOs;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

[ApiController]
[Route("[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (ok, error, data) = await _authService.RegisterAsync(dto);
        if (!ok) return BadRequest(new { message = error });
        return Ok(data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (ok, error, data) = await _authService.LoginAsync(dto);
        if (!ok) return Unauthorized(new { message = error });
        return Ok(data);
    }

    [Authorize]
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? Guid.Empty.ToString());

        var (ok, error) = await _authService.ChangePasswordAsync(userId, dto);
        if (!ok) return BadRequest(new { message = error });
        return NoContent();
    }

    [Authorize]
    [HttpPut("perfil")]
    public async Task<IActionResult> UpdatePerfil([FromBody] UpdateNombreDto dto)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? Guid.Empty.ToString());

        var (ok, error, data) = await _authService.UpdateNombreAsync(userId, dto);
        if (!ok) return BadRequest(new { message = error });
        return Ok(data);
    }
}
