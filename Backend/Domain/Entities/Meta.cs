namespace Finanzas.Domain.Entities
{
    public class Meta
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoObjetivo { get; set; }
        public decimal MontoActual { get; set; }
        public DateTime? FechaLimite { get; set; }
        public bool Completada { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}
