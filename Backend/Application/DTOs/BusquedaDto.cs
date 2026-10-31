namespace Finanzas.Application.DTOs
{
    /// <summary>Un movimiento encontrado por la búsqueda global (Ctrl/Cmd + K).</summary>
    public class ResultadoBusquedaDto
    {
        /// <summary>"GASTO" o "INGRESO".</summary>
        public string Tipo { get; set; } = string.Empty;
        public Guid Id { get; set; }
        /// <summary>Gasto: categoría. Ingreso: concepto.</summary>
        public string Titulo { get; set; } = string.Empty;
        /// <summary>Gasto: detalle. Ingreso: vacío.</summary>
        public string? Detalle { get; set; }
        public decimal Monto { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public DateTime? Fecha { get; set; }
        /// <summary>Solo gastos: "FIJO" o "VARIABLE".</summary>
        public string? TipoGasto { get; set; }
    }
}
