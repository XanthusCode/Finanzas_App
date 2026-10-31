using System.Globalization;
using System.Text.RegularExpressions;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;

namespace Finanzas.Application.Services
{
    /// <summary>
    /// Búsqueda global de movimientos (gastos e ingresos) para la paleta Ctrl/Cmd + K.
    /// Acepta texto ("netflix") o un monto ("$50.000", "50000", "50k").
    /// </summary>
    public partial class BusquedaService(IGastosRepository gastosRepo, IIngresosRepository ingresosRepo)
    {
        private const int Limite = 20;

        public async Task<IEnumerable<ResultadoBusquedaDto>> BuscarAsync(string? q, Guid userId)
        {
            var consulta = q?.Trim() ?? string.Empty;
            if (consulta.Length < 2) return [];

            var monto = ParsearMonto(consulta);
            // Si la consulta es solo un monto, no se busca como texto ("50.000" no aparece en detalles)
            string? texto = monto is null ? consulta : null;

            var gastos   = await gastosRepo.BuscarAsync(texto, monto, userId, Limite);
            var ingresos = await ingresosRepo.BuscarAsync(texto, monto, userId, Limite);

            return gastos
                .Select(g => new ResultadoBusquedaDto
                {
                    Tipo = "GASTO", Id = g.Id, Titulo = g.Categoria, Detalle = g.Detalle,
                    Monto = g.Monto, Mes = g.Mes, Anio = g.Anio, Fecha = g.Fecha, TipoGasto = g.Tipo.ToString(),
                })
                .Concat(ingresos.Select(i => new ResultadoBusquedaDto
                {
                    Tipo = "INGRESO", Id = i.Id, Titulo = i.Concepto,
                    Monto = i.Monto, Mes = i.Mes, Anio = i.Anio,
                }))
                .OrderByDescending(r => r.Anio)
                .ThenByDescending(r => r.Mes)
                .Take(Limite)
                .ToList();
        }

        // "$50.000" | "50000" | "50,000" | "50k" | "1.2m" → 50000 / 1200000. Si hay letras distintas de k/m, no es monto.
        internal static decimal? ParsearMonto(string s)
        {
            var limpio = s.Replace("$", "").Replace(" ", "").ToLowerInvariant();
            var m = MontoRegex().Match(limpio);
            if (!m.Success) return null;

            var numero = m.Groups["num"].Value;
            var sufijo = m.Groups["suf"].Value;

            if (sufijo.Length > 0)
            {
                // Con sufijo el separador es decimal: "1,5k" = 1500
                if (!decimal.TryParse(numero.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var baseNum))
                    return null;
                return baseNum * (sufijo == "k" ? 1_000m : 1_000_000m);
            }

            // Sin sufijo, puntos y comas son separadores de miles (formato COP)
            return decimal.TryParse(numero.Replace(".", "").Replace(",", ""), NumberStyles.None, CultureInfo.InvariantCulture, out var valor)
                ? valor
                : null;
        }

        [GeneratedRegex(@"^(?<num>\d[\d.,]*)(?<suf>k|m)?$")]
        private static partial Regex MontoRegex();
    }
}
