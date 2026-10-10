using System.Security;
using System.Security.Cryptography;
using System.Text;
using Finanzas.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finanzas.API;

// Sin [Authorize]: lo llama Twilio cuando llega un WhatsApp al número del sandbox.
// Se protege validando el header X-Twilio-Signature con el Auth Token de la cuenta.
[Route("[controller]")]
[ApiController]
public class WhatsAppController(WhatsAppService service, IConfiguration config, ILogger<WhatsAppController> logger) : ControllerBase
{
    private readonly WhatsAppService _service = service;
    private readonly IConfiguration _config = config;
    private readonly ILogger<WhatsAppController> _logger = logger;

    [HttpPost("webhook")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Webhook()
    {
        var form = await Request.ReadFormAsync();
        if (!FirmaValida(form))
        {
            _logger.LogWarning("Webhook de WhatsApp con firma inválida desde {Ip}", HttpContext.Connection.RemoteIpAddress);
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var respuesta = await _service.ProcesarMensajeAsync(form["From"].ToString(), form["Body"].ToString());
        return Content($"<Response><Message>{SecurityElement.Escape(respuesta)}</Message></Response>", "application/xml");
    }

    // https://www.twilio.com/docs/usage/security#validating-requests
    // HMAC-SHA1(AuthToken, url + params ordenados por nombre con nombre+valor concatenados), en base64.
    // La URL debe ser exactamente la configurada en Twilio: detrás de Render/ngrok Request.Scheme
    // y Request.Host no la reflejan, por eso se lee de configuración.
    private bool FirmaValida(IFormCollection form)
    {
        var authToken = _config["Twilio:AuthToken"];
        var url = _config["Twilio:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(authToken) || authToken.StartsWith("CHANGE_ME") || string.IsNullOrWhiteSpace(url))
        {
            _logger.LogError("Twilio:AuthToken o Twilio:WebhookUrl no configurados, se rechaza el webhook");
            return false;
        }

        var firma = Request.Headers["X-Twilio-Signature"].ToString();
        if (firma.Length == 0) return false;

        var sb = new StringBuilder(url);
        foreach (var kv in form.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            sb.Append(kv.Key).Append(kv.Value.ToString());

        var esperado = HMACSHA1.HashData(Encoding.UTF8.GetBytes(authToken), Encoding.UTF8.GetBytes(sb.ToString()));
        byte[] recibido;
        try { recibido = Convert.FromBase64String(firma); }
        catch (FormatException) { return false; }

        return CryptographicOperations.FixedTimeEquals(esperado, recibido);
    }
}
