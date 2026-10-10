using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services
{
    // Registra gastos a partir de mensajes de WhatsApp ("25000 almuerzo").
    // La firma de Twilio se valida en el controller; aquí solo llega el remitente ya confiable.
    // El usuario se identifica por el "From" crudo de Twilio (User.WhatsAppId), que se guarda
    // al enviar "vincular <código>" con el código generado en Perfil.
    public partial class WhatsAppService(
        IUserRepository userRepo,
        IGastosRepository gastosRepo,
        ICategoriasRepository categoriasRepo)
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IGastosRepository _gastosRepo = gastosRepo;
        private readonly ICategoriasRepository _categoriasRepo = categoriasRepo;

        private const string CategoriaPorDefecto = "Otros";
        private const string Ayuda = "Escribe el monto y la descripción, por ejemplo:\n25000 almuerzo\n25.000 taxi\n25k mercado";

        // "gasto 25.000 almuerzo", "$25000 almuerzo", "25k almuerzo", "25 mil almuerzo"
        [GeneratedRegex(@"^(?:gasto\s+)?\$?\s*(\d[\d.,]*)\s*(k|mil)?(?:\s+(.*))?$", RegexOptions.IgnoreCase)]
        private static partial Regex MensajeRegex();

        [GeneratedRegex(@"^vincular\s+(\d{6})$", RegexOptions.IgnoreCase)]
        private static partial Regex VincularRegex();

        public async Task<string> ProcesarMensajeAsync(string from, string body)
        {
            var texto = body.Trim();

            var vincular = VincularRegex().Match(texto);
            if (vincular.Success)
                return await VincularAsync(from, vincular.Groups[1].Value);

            var user = await _userRepo.GetByWhatsAppIdAsync(from);
            if (user is null)
                return "Este WhatsApp no está vinculado a ninguna cuenta. En Finanzas App > Mi perfil > WhatsApp genera un código y envíalo aquí como: vincular 123456";

            if (texto.Equals("ayuda", StringComparison.OrdinalIgnoreCase))
                return Ayuda;

            var m = MensajeRegex().Match(texto);
            if (!m.Success) return $"No entendí el mensaje.\n{Ayuda}";

            // En COP no se usan decimales: puntos y comas son separadores de miles.
            var digitos = m.Groups[1].Value.Replace(".", "").Replace(",", "");
            if (!decimal.TryParse(digitos, NumberStyles.None, CultureInfo.InvariantCulture, out var monto) || monto <= 0)
                return $"El monto no es válido.\n{Ayuda}";
            if (m.Groups[2].Success) monto *= 1000;

            var detalle = m.Groups[3].Value.Trim();
            if (detalle.Length == 0) detalle = "Gasto por WhatsApp";

            var categoria = await DetectarCategoriaAsync(detalle, user.Id);

            // Colombia es UTC-5 todo el año. Fecha a medianoche UTC del día local,
            // igual que las fechas "YYYY-MM-DD" que manda el formulario.
            var hoy = DateTime.UtcNow.AddHours(-5).Date;
            await _gastosRepo.CreateAsync(new Gasto
            {
                UserId    = user.Id,
                Categoria = categoria,
                Detalle   = detalle,
                Monto     = monto,
                Tipo      = TipoGasto.VARIABLE,
                Mes       = hoy.Month,
                Anio      = hoy.Year,
                Fecha     = DateTime.SpecifyKind(hoy, DateTimeKind.Utc)
            });

            return $"Registrado: ${monto.ToString("N0", CultureInfo.GetCultureInfo("es-CO"))} - {detalle} ({categoria})";
        }

        private async Task<string> VincularAsync(string from, string codigo)
        {
            var user = await _userRepo.GetByWhatsAppCodigoAsync(codigo);
            if (user is null) return "Código inválido o vencido. Genera uno nuevo en Mi perfil.";

            // Un chat de WhatsApp solo puede estar en una cuenta: si ya estaba en otra, se mueve.
            var anterior = await _userRepo.GetByWhatsAppIdAsync(from);
            if (anterior is not null && anterior.Id != user.Id)
            {
                anterior.WhatsAppId = null;
                await _userRepo.UpdateAsync(anterior);
            }

            user.WhatsAppId           = from;
            user.WhatsAppCodigo       = null;
            user.WhatsAppCodigoExpira = null;
            await _userRepo.UpdateAsync(user);
            return $"Listo {user.Nombre}, este WhatsApp quedó vinculado.\n{Ayuda}";
        }

        // Usa la primera categoría activa del usuario cuyo nombre aparezca en el detalle.
        private async Task<string> DetectarCategoriaAsync(string detalle, Guid userId)
        {
            var texto = SinTildes(detalle);
            var categorias = await _categoriasRepo.GetAllAsync(userId);
            var match = categorias
                .Where(c => c.Activa && !string.IsNullOrWhiteSpace(c.Nombre))
                .OrderByDescending(c => c.Nombre.Length)
                .FirstOrDefault(c => texto.Contains(SinTildes(c.Nombre)));
            return match?.Nombre ?? CategoriaPorDefecto;
        }

        private static string SinTildes(string s)
        {
            var sb = new StringBuilder();
            foreach (var ch in s.ToLowerInvariant().Normalize(NormalizationForm.FormD))
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString();
        }
    }
}
