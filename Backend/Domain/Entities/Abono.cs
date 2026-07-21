namespace Finanzas.Domain.Entities
{
    public class Abono
    {
        public Guid Id       { get; set; } = Guid.NewGuid();
        public Guid DeudaId  { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
