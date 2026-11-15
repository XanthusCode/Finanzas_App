using System.ComponentModel.DataAnnotations;

namespace Finanzas.Application.DTOs
{
    // Misma política que el frontend (schemas/index.ts): 8+ caracteres, mayúscula, minúscula y número
    public sealed class ContrasenaSeguraAttribute : RegularExpressionAttribute
    {
        public ContrasenaSeguraAttribute() : base(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,128}$") =>
            ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula y un número.";
    }

    public record RegisterDto(
        [Required(ErrorMessage = "Escribe tu nombre"), StringLength(80, ErrorMessage = "El nombre puede tener máximo 80 caracteres")] string Nombre,
        [Required(ErrorMessage = "Escribe tu correo"), EmailAddress(ErrorMessage = "El correo no es válido"), StringLength(254)] string Email,
        [Required(ErrorMessage = "Escribe una contraseña"), ContrasenaSegura] string Password);
    public record LoginDto(
        [Required(ErrorMessage = "Escribe tu correo"), StringLength(254)] string Email,
        [Required(ErrorMessage = "Escribe tu contraseña"), StringLength(128)] string Password);
    public record AuthResponseDto(string Token, string Email, string Nombre, int ExpiresIn);
    public record ChangePasswordDto(
        [Required, StringLength(128)] string CurrentPassword,
        [Required, ContrasenaSegura] string NewPassword);
    public record UpdateNombreDto(
        [Required(ErrorMessage = "Escribe tu nombre"), StringLength(80, ErrorMessage = "El nombre puede tener máximo 80 caracteres")] string Nombre);
    public record RecordatoriosPrefDto(bool Activos);
    public record WhatsAppEstadoDto(bool Vinculado);
    public record WhatsAppCodigoDto(string Codigo, DateTime ExpiraEn);
}
