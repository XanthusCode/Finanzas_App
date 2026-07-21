namespace Finanzas.Domain.Entities
{
    public class MovimientoMeta
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MetaId { get; set; }
        public decimal Monto { get; set; } // positivo = abono, negativo = retiro
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
