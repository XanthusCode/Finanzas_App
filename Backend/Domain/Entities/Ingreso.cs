namespace Finanzas.Domain.Entities
{
    public enum FrecuenciaIngreso { DIARIO, SEMANAL, QUINCENAL, MENSUAL, BIMESTRAL, TRIMESTRAL, ANUAL, PERSONALIZADO }

    public class Ingreso : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public Guid UserId { get; set; }
        public bool EsRecurrente { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? ActualizadoEn { get; set; }
        public Guid? CuentaId { get; set; }
        public FrecuenciaIngreso? Frecuencia { get; set; }
        public int? DiaPago { get; set; }
        public Guid? DeudaId { get; set; }
    }
}
