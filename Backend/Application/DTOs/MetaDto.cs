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
        public Guid? CuentaId { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime? ActualizadoEn { get; set; }
    }

    public class CrearMetaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoObjetivo { get; set; }
        public DateTime? FechaLimite { get; set; }
        public Guid? CuentaId { get; set; }
    }

    public class EditarMetaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoObjetivo { get; set; }
        public DateTime? FechaLimite { get; set; }
        public Guid? CuentaId { get; set; }
    }

    public class MovimientoMetaDto
    {
        public Guid Id { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }

    // Monto firmado: positivo = abono, negativo = retiro.
    public class CrearMovimientoMetaDto
    {
        public decimal Monto { get; set; }

        // Abono: cuenta desde la que sale el dinero hacia la meta (opcional).
        public Guid? CuentaOrigenId { get; set; }

        // Retiro: "SIMPLE" (solo ajusta la meta), "GASTO" (crea un gasto) o
        // "TRANSFERENCIA" (mueve el dinero a otra cuenta). Default: SIMPLE.
        public string? TipoRetiro { get; set; }

        // Retiro tipo GASTO
        public string? Categoria { get; set; }
        public string? Detalle { get; set; }

        // Retiro tipo TRANSFERENCIA
        public Guid? CuentaDestinoId { get; set; }
    }
}
