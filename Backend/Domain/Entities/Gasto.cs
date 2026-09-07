namespace Finanzas.Domain.Entities
{
    public class Gasto : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Categoria { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public TipoGasto Tipo { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public DateTime? Fecha { get; set; }
        public Guid UserId { get; set; }
        public bool EsRecurrente { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? ActualizadoEn { get; set; }
        public int? NumCuotas { get; set; }
        public int? CuotaActual { get; set; }
        public Guid? GastoOrigenId { get; set; }
        public Guid? CuentaId { get; set; }
        public Guid? DeudaId { get; set; }
    }

    public enum TipoGasto
    {
        FIJO,
        VARIABLE
    }

    public class ReciboExtraidoDto
    {
        public decimal Monto { get; set; }
        public string? Comercio { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? Fecha { get; set; }
        public string? CategoriaSugerida { get; set; }
    }
}
