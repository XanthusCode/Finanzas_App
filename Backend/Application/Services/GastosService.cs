using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Finanzas.Application.Services
{
    public class GastosService(IGastosRepository repo, IMapper mapper, GroqService groqService)
    {
        private readonly IGastosRepository _repo = repo;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<GastoDto>> GetByMesAsync(int mes, int anio, Guid userId)
        {
            var gastos = await _repo.GetByMesAsync(mes, anio, userId);
            return _mapper.Map<IEnumerable<GastoDto>>(gastos);
        }

        public async Task<GastoDto> CreateAsync(CrearGastoDto dto, Guid userId)
        {
            var numCuotas = dto.NumCuotas ?? 1;

            if (numCuotas <= 1)
            {
                var gasto = _mapper.Map<Gasto>(dto);
                gasto.UserId = userId;
                return _mapper.Map<GastoDto>(await _repo.CreateAsync(gasto));
            }

            // Genera N gastos (uno por mes) con el mismo origenId
            var origenId = Guid.NewGuid();
            Gasto? primero = null;

            for (int i = 0; i < numCuotas; i++)
            {
                int mes  = dto.Mes + i;
                int anio = dto.Anio;
                while (mes > 12) { mes -= 12; anio++; }

                var g = _mapper.Map<Gasto>(dto);
                g.Id            = i == 0 ? origenId : Guid.NewGuid();
                g.UserId        = userId;
                g.Mes           = mes;
                g.Anio          = anio;
                // La fecha exacta solo aplica a la primera cuota; las siguientes caen
                // en el día 1 de su mes (fallback del heatmap) para no repetir la misma fecha.
                g.Fecha         = i == 0 ? dto.Fecha : null;
                g.NumCuotas     = numCuotas;
                g.CuotaActual   = i + 1;
                g.GastoOrigenId = origenId;

                var creado = await _repo.CreateAsync(g);
                primero ??= creado;
            }

            return _mapper.Map<GastoDto>(primero!);
        }

        public async Task<GastoDto?> UpdateAsync(Guid id, CrearGastoDto dto, Guid userId)
        {
            var existente = await _repo.GetByIdAsync(id, userId);
            if (existente is null) return null;

            _mapper.Map(dto, existente);
            var actualizado = await _repo.UpdateAsync(existente);
            return _mapper.Map<GastoDto>(actualizado);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var existente = await _repo.GetByIdAsync(id, userId);
            if (existente is null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }

        public async Task<int> ImportarAsync(IEnumerable<CrearGastoDto> dtos, int mes, int anio, Guid userId)
        {
            int count = 0;
            foreach (var dto in dtos)
            {
                var gasto = _mapper.Map<Gasto>(dto);
                gasto.UserId = userId;
                gasto.Mes    = mes;
                gasto.Anio   = anio;
                await _repo.CreateAsync(gasto);
                count++;
            }
            return count;
        }

        public async Task<ReciboExtraidoDto?> EscanearReciboAsync(IFormFile recibo)
        {
            using var ms = new MemoryStream();
            await recibo.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var json = await groqService.GenerarConImagenAsync(PromptRecibo, bytes, recibo.ContentType);
            if (json is null) return null;

            return JsonSerializer.Deserialize<ReciboExtraidoDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<IEnumerable<ResumenCategoriaDto>> GetResumenCategoriaAsync(int mes, int anio, Guid userId) =>
            await _repo.GetResumenCategoriaAsync(mes, anio, userId);

        // Gastos recurrentes con 3+ meses activos este año: candidatos a "suscripción olvidada"
        // que el usuario debería reconsiderar. Reutiliza el flag EsRecurrente que ya existe,
        // sin agregar columnas ni tracking nuevo.
        public async Task<IEnumerable<SuscripcionDto>> GetSuscripcionesAsync(Guid userId)
        {
            var gastos = await _repo.GetByAnioAsync(DateTime.UtcNow.Year, userId);

            return gastos
                .Where(g => g.EsRecurrente)
                .GroupBy(g => (g.Categoria, g.Detalle, g.Monto))
                .Select(grp => new SuscripcionDto
                {
                    Categoria = grp.Key.Categoria,
                    Detalle = grp.Key.Detalle,
                    Monto = grp.Key.Monto,
                    MesesActivo = grp.Select(g => g.Mes).Distinct().Count()
                })
                .Where(s => s.MesesActivo >= 3)
                .OrderByDescending(s => s.MesesActivo)
                .ToList();
        }

        // Heatmap anual estilo GitHub: un total por día. Los gastos creados antes de que
        // existiera el campo Fecha caen en el día 1 de su mes como fallback.
        public async Task<IEnumerable<HeatmapDiaDto>> GetHeatmapAsync(int anio, Guid userId)
        {
            var gastos = await _repo.GetByAnioAsync(anio, userId);

            return gastos
                .GroupBy(g => (g.Fecha ?? new DateTime(g.Anio, g.Mes, 1)).Date)
                .Select(grp => new HeatmapDiaDto { Fecha = grp.Key, Total = grp.Sum(g => g.Monto) })
                .OrderBy(d => d.Fecha)
                .ToList();
        }

        public async Task<IEnumerable<GastoDto>> GetCuotasAsync(Guid userId)
        {
            var cuotas = await _repo.GetCuotasAsync(userId);
            return _mapper.Map<IEnumerable<GastoDto>>(cuotas);
        }

        public async Task<IEnumerable<GastoDto>> CopiarRecurrentesAsync(int mes, int anio, Guid userId)
        {
            var mesPrevio  = mes == 1 ? 12 : mes - 1;
            var anioPrevio = mes == 1 ? anio - 1 : anio;
            var recurrentes = await _repo.GetRecurrentesAsync(mesPrevio, anioPrevio, userId);

            var creados = new List<GastoDto>();
            foreach (var r in recurrentes)
            {
                var nuevo = new Gasto
                {
                    Categoria    = r.Categoria,
                    Detalle      = r.Detalle,
                    Monto        = r.Monto,
                    Tipo         = r.Tipo,
                    Mes          = mes,
                    Anio         = anio,
                    EsRecurrente = true,
                    UserId       = userId,
                    CuentaId     = r.CuentaId
                };
                var creado = await _repo.CreateAsync(nuevo);
                creados.Add(_mapper.Map<GastoDto>(creado));
            }
            return creados;
        }

        private const string PromptRecibo = """
            Eres un sistema que extrae datos de recibos o facturas de compra a partir de una imagen.
            Analiza la imagen y devuelve ÚNICAMENTE un JSON válido (sin texto adicional, sin markdown,
            sin explicaciones) con esta estructura exacta:

            {
              "monto": number,
              "comercio": string,
              "descripcion": string,
              "fecha": string | null,
              "categoriaSugerida": string
            }

            Donde:
            - "monto": el total final pagado, solo el número (sin símbolos de moneda ni separadores de miles).
            - "comercio": nombre del establecimiento que aparece en el recibo.
            - "descripcion": una descripción breve y clara de qué se compró (el producto o artículo
              principal del recibo, no el nombre del comercio). Ej: "Bolsa de basura", no "Ara".
            - "fecha": fecha de la compra en formato YYYY-MM-DD si aparece; si no aparece, null.
            - "categoriaSugerida": una palabra que describa el tipo de gasto (ej: "Comida", "Transporte").

            Reglas:
            - El monto es obligatorio. Si no puedes leerlo con certeza, usa 0.
            - Los demás campos: si no puedes leerlos con certeza, exepto en la categoria sugerida usa null.
            - No inventes datos que no estén visibles en la imagen.
            - Responde solo con el JSON, nada más.
            """;
    }
}
