using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Finanzas.Application.Services
{
    public class RecordatoriosService(
        IUserRepository userRepo,
        DeudaService deudaService,
        IPresupuestosRepository presupuestosRepo,
        GastosService gastosService,
        ResumenService resumenService,
        EmailService emailService,
        IConfiguration config)
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly DeudaService _deudaService = deudaService;
        private readonly IPresupuestosRepository _presupuestosRepo = presupuestosRepo;
        private readonly GastosService _gastosService = gastosService;
        private readonly ResumenService _resumenService = resumenService;
        private readonly EmailService _emailService = emailService;
        private readonly IConfiguration _config = config;

        // Mismos umbrales que DeudasAlerta.vue / PresupuestosAlerta.vue en el frontend.
        private const int DiasProximaVencer = 7;
        private const int PctWarn = 80;

        public async Task<int> ProcesarAsync()
        {
            var usuarios = await _userRepo.GetAllAsync();
            var hoy = DateTime.UtcNow;
            var enviados = 0;

            foreach (var user in usuarios)
            {
                var deudas = await _deudaService.GetAllAsync(user.Id);
                var pendientes = deudas.Where(d => !d.Pagada && d.FechaLimite.HasValue).ToList();
                var vencidas = pendientes.Where(d => d.FechaLimite!.Value < hoy).ToList();
                var proximas = pendientes
                    .Where(d => d.FechaLimite!.Value >= hoy && (d.FechaLimite!.Value - hoy).TotalDays <= DiasProximaVencer)
                    .ToList();

                var (presupuestosEnRiesgo, totalPresupuestos) = await GetPresupuestosAsync(user.Id, hoy);

                if (vencidas.Count == 0 && proximas.Count == 0 && presupuestosEnRiesgo.Count == 0)
                    continue;

                var resumenMes = await _resumenService.GetAsync(hoy.Month, hoy.Year, user.Id);
                var mensajePositivo = GetMensajePositivo(resumenMes, totalPresupuestos, presupuestosEnRiesgo.Count);

                var frontendUrl = _config["FRONTEND_URL"];
                var html = ConstruirHtml(user.Nombre, hoy, vencidas, proximas, presupuestosEnRiesgo, mensajePositivo, resumenMes, frontendUrl);
                await _emailService.EnviarAsync(user.Email, "Tienes pendientes en Finanzas App", html);
                enviados++;
            }

            return enviados;
        }

        private async Task<(List<(string Categoria, int Pct, bool Danger)> EnRiesgo, int Total)> GetPresupuestosAsync(Guid userId, DateTime hoy)
        {
            var presupuestos = await _presupuestosRepo.GetAllAsync(userId);
            var conLimite = presupuestos.Where(p => p.Limite > 0).ToList();
            var enRiesgo = new List<(string, int, bool)>();
            if (conLimite.Count == 0) return (enRiesgo, 0);

            var resumen = await _gastosService.GetResumenCategoriaAsync(hoy.Month, hoy.Year, userId);
            var gastoPorCategoria = resumen.ToDictionary(r => r.Categoria, r => r.Total);

            foreach (var p in conLimite)
            {
                var gastado = gastoPorCategoria.GetValueOrDefault(p.Categoria);
                var pct = (int)Math.Round(gastado / p.Limite * 100);
                if (pct >= PctWarn) enRiesgo.Add((p.Categoria, pct, pct > 100));
            }

            return (enRiesgo, conLimite.Count);
        }

        private record MensajePositivo(string Titulo, string? ValorGrande, string Descripcion);

        // Mismos umbrales que el "health-badge" del Dashboard (Excelente/Muy bien/Bien/Justo/Déficit).
        // Nunca inventa una buena noticia: si no hay ninguna señal positiva real, devuelve null
        // y el email simplemente no incluye esa sección.
        private static MensajePositivo? GetMensajePositivo(ResumenDto resumen, int totalPresupuestos, int enRiesgoCount)
        {
            if (resumen.TotalIngresos > 0)
            {
                if (resumen.PctAhorro >= 30) return new("Excelente trabajo", $"{resumen.PctAhorro}%", "de tus ingresos ahorrado este mes.");
                if (resumen.PctAhorro >= 20) return new("Muy buen trabajo", $"{resumen.PctAhorro}%", "de tus ingresos ahorrado este mes.");
                if (resumen.PctAhorro >= 10) return new("Vas bien", $"{resumen.PctAhorro}%", "de tus ingresos ahorrado este mes.");
                if (resumen.PctAhorro >= 0)  return new("Mes en positivo", null, "Cerraste el mes sin números rojos. Sigue así.");
            }

            var enOrden = totalPresupuestos - enRiesgoCount;
            if (enOrden > 0)
                return new("Buen manejo del presupuesto", $"{enOrden}",
                    $"presupuesto{(enOrden > 1 ? "s" : "")} bien encaminado{(enOrden > 1 ? "s" : "")} este mes.");

            return null;
        }

        private static string DiasTexto(DateTime fecha, DateTime hoy, bool vencida)
        {
            var dias = (int)Math.Abs((fecha.Date - hoy.Date).TotalDays);
            if (vencida) return dias == 0 ? "Venció hoy" : $"Venció hace {dias} día{(dias == 1 ? "" : "s")}";
            return dias == 0 ? "Vence hoy" : $"Vence en {dias} día{(dias == 1 ? "" : "s")}";
        }

        // Paleta y estructura pedidas por el usuario: fondo/tarjeta más suaves, filas con
        // fondo neutro (solo borde lateral de color), resumen rápido arriba, mensaje positivo
        // como bloque protagonista, botón de acción y métricas del mes. Sin CSS variables ni
        // color-mix (no los resuelven los clientes de correo) — todo hex fijo y tablas, porque
        // Outlook/Gmail ignoran o rompen flex, grid y CSS moderno.
        private const string ColorBgPage        = "#070b14";
        private const string ColorSurface       = "#111827";
        private const string ColorRowBg         = "#0c1420";
        private const string ColorBorder        = "#23324a";
        private const string ColorAccent        = "#4f7cff";
        private const string ColorGreen         = "#22c55e";
        private const string ColorRed           = "#f87171";
        private const string ColorAmber         = "#fbbf24";
        private const string ColorTextPrimary   = "#f8fafc";
        private const string ColorTextSecondary = "#94a3b8";
        private const string FontBody = "-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif";

        private static string ConstruirHtml(
            string nombre,
            DateTime hoy,
            List<DeudaDto> vencidas,
            List<DeudaDto> proximas,
            List<(string Categoria, int Pct, bool Danger)> presupuestosEnRiesgo,
            MensajePositivo? mensajePositivo,
            ResumenDto resumenMes,
            string? frontendUrl)
        {
            var resumenLineas = new List<string>();
            if (vencidas.Count > 0) resumenLineas.Add($"🔴 {vencidas.Count} deuda{(vencidas.Count > 1 ? "s" : "")} vencida{(vencidas.Count > 1 ? "s" : "")}");
            if (proximas.Count > 0) resumenLineas.Add($"🟡 {proximas.Count} deuda{(proximas.Count > 1 ? "s" : "")} próxima{(proximas.Count > 1 ? "s" : "")} a vencer");
            if (presupuestosEnRiesgo.Count > 0) resumenLineas.Add($"📊 {presupuestosEnRiesgo.Count} presupuesto{(presupuestosEnRiesgo.Count > 1 ? "s" : "")} en riesgo");

            var bloques = new List<string> { ResumenRapido(resumenLineas) };

            if (mensajePositivo is not null) bloques.Add(HeroPositivo(mensajePositivo));

            if (vencidas.Count > 0)
                bloques.Add(Seccion("🔴", "Deudas vencidas", vencidas.Select(d =>
                    FilaDeuda(d.Persona, d.Descripcion, DiasTexto(d.FechaLimite!.Value, hoy, true), ColorRed))));

            if (proximas.Count > 0)
                bloques.Add(Seccion("🟡", "Próximas a vencer", proximas.Select(d =>
                    FilaDeuda(d.Persona, d.Descripcion, DiasTexto(d.FechaLimite!.Value, hoy, false), ColorAmber))));

            if (presupuestosEnRiesgo.Count > 0)
                bloques.Add(Seccion("📊", "Presupuestos en riesgo", presupuestosEnRiesgo.Select(p =>
                    FilaPresupuesto(p.Categoria, $"{p.Pct}%{(p.Danger ? " · excedido" : "")}", p.Danger ? ColorRed : ColorAmber))));

            if (resumenMes.TotalIngresos > 0 || resumenMes.TotalGastos > 0)
                bloques.Add(Metricas(resumenMes));

            if (!string.IsNullOrWhiteSpace(frontendUrl))
                bloques.Add(BotonAccion($"{frontendUrl.TrimEnd('/')}/dashboard"));

            return $"""
                <html>
                <head>
                <meta name="color-scheme" content="dark">
                <meta name="supported-color-schemes" content="dark">
                </head>
                <body style="margin:0;padding:0;background:{ColorBgPage};font-family:{FontBody};">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" bgcolor="{ColorBgPage}" style="background:{ColorBgPage};padding:32px 16px;">
                <tr><td align="center">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" bgcolor="{ColorSurface}" style="max-width:560px;background:{ColorSurface};border-radius:16px;border:1px solid {ColorBorder};">
                <tr><td align="center" style="padding:32px 32px 20px;">
                <span style="font-family:{FontBody};font-weight:800;font-size:19px;letter-spacing:0.06em;color:{ColorTextPrimary};">FINANZAS</span><br>
                <span style="font-size:12.5px;color:{ColorTextSecondary};">Tu resumen financiero de hoy</span>
                </td></tr>
                <tr><td style="padding:0 32px;"><div style="height:1px;background:{ColorBorder};"></div></td></tr>
                <tr><td style="padding:24px 32px 4px;">
                <p style="margin:0 0 4px;font-size:15px;font-weight:600;color:{ColorTextPrimary};">Hola {nombre} 👋</p>
                <p style="margin:0;font-size:13px;color:{ColorTextSecondary};">Encontramos algunos elementos que requieren tu atención.</p>
                </td></tr>
                {string.Join("", bloques)}
                <tr><td style="padding:8px 32px 28px;" align="center">
                <p style="margin:0 0 4px;font-size:13px;font-weight:700;color:{ColorTextPrimary};">Finanzas App</p>
                <p style="margin:0;font-size:11.5px;color:{ColorTextSecondary};">Organiza tus finanzas, presupuestos y metas en un solo lugar.</p>
                </td></tr>
                </table>
                </td></tr>
                </table>
                </body>
                </html>
                """;
        }

        private static string ResumenRapido(List<string> lineas)
        {
            if (lineas.Count == 0) return "";
            var filas = string.Join("", lineas.Select(l => $"""
                <p style="margin:0 0 6px;font-size:13.5px;color:{ColorTextPrimary};">{l}</p>
                """));
            return $"""
                <tr><td style="padding:20px 32px 4px;">
                <p style="margin:0 0 10px;font-size:11px;font-weight:700;letter-spacing:0.06em;text-transform:uppercase;color:{ColorTextSecondary};">Hoy tienes</p>
                {filas}
                </td></tr>
                """;
        }

        private static string HeroPositivo(MensajePositivo m)
        {
            var valor = m.ValorGrande is not null
                ? $"""<p style="margin:6px 0;font-size:38px;font-weight:800;color:{ColorGreen};line-height:1;">{m.ValorGrande}</p>"""
                : "";
            return $"""
                <tr><td style="padding:20px 32px 4px;">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" bgcolor="{ColorRowBg}" style="background:{ColorRowBg};border-radius:12px;border:1px solid {ColorBorder};">
                <tr><td align="center" style="padding:22px 20px;">
                <p style="margin:0;font-size:12px;font-weight:700;letter-spacing:0.04em;color:{ColorGreen};">✓ {m.Titulo}</p>
                {valor}
                <p style="margin:0;font-size:13px;color:{ColorTextSecondary};">{m.Descripcion}</p>
                </td></tr>
                </table>
                </td></tr>
                """;
        }

        private static string Seccion(string emoji, string titulo, IEnumerable<string> filas) => $"""
            <tr><td style="padding:22px 32px 4px;">
            <p style="margin:0 0 10px;font-size:14px;font-weight:600;color:{ColorTextPrimary};">{emoji} {titulo}</p>
            {string.Join("", filas)}
            </td></tr>
            """;

        private static string FilaDeuda(string persona, string descripcion, string estado, string colorAcento) => $"""
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" bgcolor="{ColorRowBg}" style="margin-bottom:8px;background:{ColorRowBg};border-radius:8px;border-left:3px solid {colorAcento};">
            <tr><td style="padding:10px 14px;">
            <p style="margin:0 0 2px;font-size:13.5px;font-weight:600;color:{ColorTextPrimary};">{persona}</p>
            <p style="margin:0 0 4px;font-size:12.5px;color:{ColorTextSecondary};">{descripcion}</p>
            <p style="margin:0;font-size:12px;font-weight:600;color:{colorAcento};">{estado}</p>
            </td></tr>
            </table>
            """;

        private static string FilaPresupuesto(string categoria, string pctTexto, string colorAcento) => $"""
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" bgcolor="{ColorRowBg}" style="margin-bottom:8px;background:{ColorRowBg};border-radius:8px;border-left:3px solid {colorAcento};">
            <tr>
            <td style="padding:10px 14px;font-size:13.5px;color:{ColorTextPrimary};">{categoria}</td>
            <td align="right" style="padding:10px 14px;font-size:13px;font-weight:700;color:{colorAcento};white-space:nowrap;">{pctTexto}</td>
            </tr>
            </table>
            """;

        private static string Metricas(ResumenDto r) => $"""
            <tr><td style="padding:22px 32px 4px;">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
            <tr>
            <td align="center" style="padding:12px 4px;border-top:1px solid {ColorBorder};">
            <p style="margin:0 0 3px;font-size:10.5px;letter-spacing:0.06em;text-transform:uppercase;color:{ColorTextSecondary};">Ingresos</p>
            <p style="margin:0;font-size:14px;font-weight:700;color:{ColorTextPrimary};">{FmtCop(r.TotalIngresos)}</p>
            </td>
            <td align="center" style="padding:12px 4px;border-top:1px solid {ColorBorder};">
            <p style="margin:0 0 3px;font-size:10.5px;letter-spacing:0.06em;text-transform:uppercase;color:{ColorTextSecondary};">Gastos</p>
            <p style="margin:0;font-size:14px;font-weight:700;color:{ColorTextPrimary};">{FmtCop(r.TotalGastos)}</p>
            </td>
            <td align="center" style="padding:12px 4px;border-top:1px solid {ColorBorder};">
            <p style="margin:0 0 3px;font-size:10.5px;letter-spacing:0.06em;text-transform:uppercase;color:{ColorTextSecondary};">Ahorro</p>
            <p style="margin:0;font-size:14px;font-weight:700;color:{ColorTextPrimary};">{r.PctAhorro}%</p>
            </td>
            </tr>
            </table>
            </td></tr>
            """;

        private static string FmtCop(decimal monto) => "$" + Math.Round(monto).ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-CO"));

        private static string BotonAccion(string url) => $"""
            <tr><td align="center" style="padding:26px 32px 4px;">
            <table role="presentation" cellpadding="0" cellspacing="0">
            <tr><td align="center" bgcolor="{ColorAccent}" style="background:{ColorAccent};border-radius:8px;">
            <a href="{url}" style="display:inline-block;padding:12px 28px;font-size:13.5px;font-weight:700;color:#ffffff;text-decoration:none;">Revisar ahora →</a>
            </td></tr>
            </table>
            </td></tr>
            """;
    }
}
