using Finanzas.Application.Interfaces;

namespace Finanzas.Application.Services
{
    // Arma un resumen agregado (nunca transacciones crudas) y se lo manda a Groq para que
    // devuelva observaciones en lenguaje natural. Es a demanda (el usuario aprieta un botón),
    // no automático, para controlar el volumen de llamadas a la API.
    public class InsightsService(
        ResumenService resumenService,
        GastosService gastosService,
        IPresupuestosRepository presupuestosRepo,
        GroqService groqService)
    {
        private readonly ResumenService _resumenService = resumenService;
        private readonly GastosService _gastosService = gastosService;
        private readonly IPresupuestosRepository _presupuestosRepo = presupuestosRepo;
        private readonly GroqService _groqService = groqService;

        private const string SystemPrompt = """
            Eres un amigo colombiano que entiende de plata y le comenta a alguien cómo le fue
            con sus finanzas este mes — NO un asesor financiero escribiendo un informe. Con los
            datos que te dan, escribe entre 2 y 4 observaciones cortas (máximo 2 líneas cada una).

            Reglas de estilo, muy importantes:
            - Habla como en una conversación normal, no como un reporte. Nada de "lo que sugiere
              que", "capacidad de ahorro", "asignación de ingresos" ni palabras rebuscadas.
            - Sé directo con los problemas: en vez de "podrías revisar tus hábitos en esta
              categoría", di algo como "te pasaste con juegos" o "en moto gastaste casi 4 veces
              lo que tenías presupuestado".
            - Usa cifras concretas pero explicadas fácil (ej. "ahorraste $400.000 este mes" en vez
              de "tu tasa de ahorro fue del 28%").
            - No repitas la misma estructura de frase en cada observación.
            - No des consejos genéricos de "haz un presupuesto" si el usuario ya tiene uno definido.

            Responde solo con las observaciones, una por línea, sin encabezados, sin numeración
            y sin viñetas.
            """;

        // huboDatos=false: no hay suficiente info financiera este mes, ni se intentó llamar a Groq.
        // huboDatos=true, texto=null: sí había datos, pero la llamada a Groq falló (ver logs de GroqService).
        public async Task<(bool huboDatos, string? texto)> GenerarAsync(int mes, int anio, Guid userId)
        {
            var mesAnterior  = mes == 1 ? 12 : mes - 1;
            var anioAnterior = mes == 1 ? anio - 1 : anio;

            var actual    = await _resumenService.GetAsync(mes, anio, userId);
            var anterior  = await _resumenService.GetAsync(mesAnterior, anioAnterior, userId);

            if (actual.TotalIngresos <= 0 && actual.TotalGastos <= 0)
                return (false, null);

            var categorias   = (await _gastosService.GetResumenCategoriaAsync(mes, anio, userId))
                .OrderByDescending(c => c.Total)
                .Take(8)
                .ToList();
            var presupuestos = (await _presupuestosRepo.GetAllAsync(userId))
                .Where(p => p.Limite > 0)
                .ToList();

            var categoriasTexto = categorias.Count == 0
                ? "(sin gastos registrados este mes)"
                : string.Join("\n", categorias.Select(c => $"- {c.Categoria}: ${c.Total:N0}"));

            var presupuestosTexto = presupuestos.Count == 0
                ? "(sin presupuestos definidos)"
                : string.Join("\n", presupuestos.Select(p =>
                {
                    var gastado = categorias.FirstOrDefault(c => c.Categoria == p.Categoria)?.Total ?? 0;
                    var pct = p.Limite > 0 ? Math.Round(gastado / p.Limite * 100) : 0;
                    return $"- {p.Categoria}: {pct}% del límite (${gastado:N0} de ${p.Limite:N0})";
                }));

            var prompt = $"""
                Datos financieros del usuario para {mes}/{anio} (montos en pesos colombianos COP):

                Mes actual:
                - Ingresos: ${actual.TotalIngresos:N0}
                - Gastos fijos: ${actual.TotalFijos:N0}
                - Gastos ocasionales: ${actual.TotalVariables:N0}
                - Ahorro: ${actual.Ahorro:N0} ({actual.PctAhorro}% de los ingresos)

                Mes anterior ({mesAnterior}/{anioAnterior}):
                - Ingresos: ${anterior.TotalIngresos:N0}
                - Gastos totales: ${anterior.TotalGastos:N0}
                - Ahorro: {anterior.PctAhorro}%

                Gasto por categoría este mes (top 8):
                {categoriasTexto}

                Presupuestos definidos:
                {presupuestosTexto}
                """;

            var texto = await _groqService.GenerarTextoAsync(SystemPrompt, prompt);
            return (true, texto);
        }
    }
}
