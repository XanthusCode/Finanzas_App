namespace Finanzas.Domain.Entities
{
    public class Presupuesto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Categoria { get; set; } = string.Empty;
        public decimal Limite { get; set; }
        public Guid UserId { get; set; }
    }
}
