namespace Finanzas.Domain.Entities
{
    public class Ingreso
    {
        public int Id { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}
