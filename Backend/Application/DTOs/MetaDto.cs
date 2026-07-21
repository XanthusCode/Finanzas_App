using Finanzas.Domain.Entities;

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
        public DateTime CreadoEn { get; set; }
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

    public class MovimientoMetaDto
    {
        public Guid Id { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class CrearMovimientoMetaDto
    {
        public decimal Monto { get; set; }
    }
}
