namespace Finanzas.Application.DTOs
{
    public record RegisterDto(string Nombre, string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record AuthResponseDto(string Token, string Email, string Nombre, int ExpiresIn);
    public record ChangePasswordDto(string CurrentPassword, string NewPassword);
    public record UpdateNombreDto(string Nombre);
    public record RecordatoriosPrefDto(bool Activos);
    public record WhatsAppEstadoDto(bool Vinculado);
    public record WhatsAppCodigoDto(string Codigo, DateTime ExpiraEn);
}
