using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Finanzas.Application.Services
{
    public class GroqService(HttpClient http, IConfiguration config, ILogger<GroqService> logger)
    {
        private readonly HttpClient _http = http;
        private readonly IConfiguration _config = config;
        private readonly ILogger<GroqService> _logger = logger;

        public async Task<string?> GenerarTextoAsync(string systemPrompt, string userPrompt)
        {
            var apiKey = _config["Groq:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("CHANGE_ME"))
            {
                _logger.LogWarning("Groq:ApiKey no configurado, se omite la generación de insights");
                return null;
            }

            var modelo = _config["Groq:Model"] ?? "llama-3.3-70b-versatile";

            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
            {
                Content = JsonContent.Create(new
                {
                    model = modelo,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user",   content = userPrompt }
                    },
                    temperature = 0.6
                })
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var res = await _http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError("Groq respondió {Status}: {Body}", res.StatusCode, body);
                return null;
            }

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }

        public async Task<string?> GenerarConImagenAsync(string systemPrompt, byte[] imagenBytes, string mimeType)
        {
            var apiKey = _config["Groq:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("CHANGE_ME"))
            {
                _logger.LogWarning("Groq:ApiKey no configurado, se omite el escaneo de recibo");
                return null;
            }

            var modelo = _config["Groq:VisionModel"] ?? "qwen/qwen3.6-27b";
            var base64 = Convert.ToBase64String(imagenBytes);
            var dataUri = $"data:{mimeType};base64,{base64}";

            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
            {
                Content = JsonContent.Create(new
                {
                    model = modelo,
                    response_format = new { type = "json_object" },
                    messages = new object[]
                    {
                new { role = "system", content = systemPrompt },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "image_url", image_url = new { url = dataUri } }
                    }
                }
                    },
                    temperature = 0.6
                })
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var res = await _http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError("Groq (visión) respondió {Status}: {Body}", res.StatusCode, body);
                return null;
            }

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
    }
}
