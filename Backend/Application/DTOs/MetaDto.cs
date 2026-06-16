namespace Finanzas.Application.DTOs
{
    public class MetaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoObjetivo { get; set; }
        public decimal MontoActual { get; set; }
        public DateTime? FechaLimite { get; set; }
        public bool Completada { get; set; }
    }

    public class CrearMetaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoObjetivo { get; set; }
        public DateTime? FechaLimite { get; set; }
    }

    public class EditarMetaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoObjetivo { get; set; }
        public DateTime? FechaLimite { get; set; }
    }

    public class AbonarMetaDto
    {
        public decimal Monto { get; set; }
    }
}
