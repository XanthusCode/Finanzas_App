using System.ComponentModel.DataAnnotations;
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
        [Required(ErrorMessage = "Escribe un nombre"), StringLength(80)]
        public string Nombre { get; set; } = string.Empty;
        [Range(0.01, 9999999999.99, ErrorMessage = "El objetivo debe ser mayor a $0")]
        public decimal MontoObjetivo { get; set; }
        public DateTime? FechaLimite { get; set; }
        public Guid? CuentaId { get; set; }
    }

    public class EditarMetaDto
    {
        [Required(ErrorMessage = "Escribe un nombre"), StringLength(80)]
        public string Nombre { get; set; } = string.Empty;
        [Range(0.01, 9999999999.99, ErrorMessage = "El objetivo debe ser mayor a $0")]
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
        [Range(-9999999999.99, 9999999999.99)]
        public decimal Monto { get; set; }

        // Abono: cuenta desde la que sale el dinero hacia la meta (opcional).
        public Guid? CuentaOrigenId { get; set; }

        // Retiro: "SIMPLE" (solo ajusta la meta), "GASTO" (crea un gasto) o
        // "TRANSFERENCIA" (mueve el dinero a otra cuenta). Default: SIMPLE.
        [RegularExpression("^(SIMPLE|GASTO|TRANSFERENCIA)$", ErrorMessage = "Tipo de retiro no válido")]
        public string? TipoRetiro { get; set; }

        // Retiro tipo GASTO
        [StringLength(100)]
        public string? Categoria { get; set; }
        [StringLength(150)]
        public string? Detalle { get; set; }

        // Retiro tipo TRANSFERENCIA
        public Guid? CuentaDestinoId { get; set; }
    }
}
