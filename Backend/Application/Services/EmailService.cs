using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Finanzas.Application.Services
{
    public class EmailService(HttpClient http, IConfiguration config, ILogger<EmailService> logger)
    {
        private readonly HttpClient _http = http;
        private readonly IConfiguration _config = config;
        private readonly ILogger<EmailService> _logger = logger;

        public async Task EnviarAsync(string destinatario, string asunto, string html)
        {
            var apiKey = _config["Resend:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("CHANGE_ME"))
            {
                _logger.LogWarning("Resend:ApiKey no configurado, se omite el envío de email a {Destinatario}", destinatario);
                return;
            }

            var from = _config["Resend:FromEmail"] ?? "Finanzas App <onboarding@resend.dev>";

            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Content = JsonContent.Create(new { from, to = new[] { destinatario }, subject = asunto, html })
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _logger.LogError("Resend respondió {Status} al enviar a {Destinatario}: {Body}", res.StatusCode, destinatario, body);
            }
        }
    }
}
