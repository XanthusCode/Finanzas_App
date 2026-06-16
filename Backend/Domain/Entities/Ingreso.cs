namespace Finanzas.Domain.Entities
{
    public class Ingreso
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public Guid UserId { get; set; }
        public bool EsRecurrente { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}
