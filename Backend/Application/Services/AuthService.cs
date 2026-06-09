using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Finanzas.Application.Services
{
    public class AuthService(IUserRepository userRepo, IConfiguration config)
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IConfiguration _config = config;

        public async Task<(bool ok, string error, AuthResponseDto? data)> RegisterAsync(RegisterDto dto)
        {
            if (await _userRepo.ExistsAsync(dto.Email))
                return (false, "El correo ya está registrado.", null);

            var user = new User
            {
                Email = dto.Email.ToLower(),
                Nombre = dto.Nombre.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreadoEn = DateTime.UtcNow
            };

            await _userRepo.CreateAsync(user);
            return (true, string.Empty, BuildResponse(user));
        }

        public async Task<(bool ok, string error, AuthResponseDto? data)> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return (false, "Credenciales incorrectas.", null);

            return (true, string.Empty, BuildResponse(user));
        }

        private AuthResponseDto BuildResponse(User user)
        {
            var token = GenerateToken(user);
            var expiresIn = int.Parse(_config["Jwt:ExpiryHours"] ?? "2") * 3600;
            return new AuthResponseDto(token, user.Email, user.Nombre, expiresIn);
        }

        public async Task<(bool ok, string error)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user is null) return (false, "Usuario no encontrado.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return (false, "La contraseña actual es incorrecta.");

            if (dto.NewPassword.Length < 6)
                return (false, "La nueva contraseña debe tener al menos 6 caracteres.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _userRepo.UpdateAsync(user);
            return (true, string.Empty);
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:ExpiryHours"] ?? "2"));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("nombre", user.Nombre),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:   _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims:   claims,
                expires:  expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
