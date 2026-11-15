using System.ComponentModel.DataAnnotations;

namespace Finanzas.Application.DTOs
{
    public class GastoDto
    {
        public Guid Id { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int Mes { get; set; }
        public int Anio { get; set; }
        public DateTime? Fecha { get; set; }
        public bool EsRecurrente { get; set; }
        public int? NumCuotas { get; set; }
        public int? CuotaActual { get; set; }
        public Guid? GastoOrigenId { get; set; }
        public Guid? CuentaId { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        // Cuándo se registró en la app (distinto de Fecha, que es el día del cobro)
        public DateTime CreadoEn { get; set; }
    }

    public class CrearGastoDto
    {
        [Required(ErrorMessage = "Selecciona una categoría"), StringLength(100)]
        public string Categoria { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;

        [Range(0.01, 9999999999.99, ErrorMessage = "El monto debe ser mayor a $0")]
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int Mes { get; set; }
        public int Anio { get; set; }
        public DateTime? Fecha { get; set; }
        public bool EsRecurrente { get; set; }
        public int? NumCuotas { get; set; }
        public Guid? CuentaId { get; set; }
    }

    public class ResumenCategoriaDto
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class SuscripcionDto
    {
        public string Categoria { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int MesesActivo { get; set; }
    }

    public class HeatmapDiaDto
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }

}
