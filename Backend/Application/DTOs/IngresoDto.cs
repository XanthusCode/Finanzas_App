using System.ComponentModel.DataAnnotations;

namespace Finanzas.Application.DTOs
{
    public class IngresoDto
    {
        public Guid Id { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public bool EsRecurrente { get; set; }
        public Guid? CuentaId { get; set; }
        public string? Frecuencia { get; set; }
        public int? DiaPago { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        public DateTime? CreadoEn { get; set; }
    }

    public class CrearIngresoDto
    {
        [Required(ErrorMessage = "Escribe un concepto"), StringLength(100)]
        public string Concepto { get; set; } = string.Empty;
        [Range(0.01, 9999999999.99, ErrorMessage = "El monto debe ser mayor a $0")]
        public decimal Monto { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public bool EsRecurrente { get; set; }
        public Guid? CuentaId { get; set; }
        public string? Frecuencia { get; set; }
        public int? DiaPago { get; set; }
    }

    public class EditarIngresoDto
    {
        [Required(ErrorMessage = "Escribe un concepto"), StringLength(100)]
        public string Concepto { get; set; } = string.Empty;
        [Range(0.01, 9999999999.99, ErrorMessage = "El monto debe ser mayor a $0")]
        public decimal Monto { get; set; }
        public bool EsRecurrente { get; set; }
        public Guid? CuentaId { get; set; }
        public string? Frecuencia { get; set; }
        public int? DiaPago { get; set; }
    }
}
