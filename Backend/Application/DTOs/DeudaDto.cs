using System.ComponentModel.DataAnnotations;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.DTOs
{
    public class DeudaDto
    {
        public Guid Id          { get; set; }
        public string Persona   { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto    { get; set; }
        public string Tipo      { get; set; } = string.Empty;
        public DateTime? FechaLimite { get; set; }
        public Guid? CuentaId   { get; set; }
        public bool Pagada      { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime? ActualizadoEn { get; set; }
        public decimal MontoPagado   { get; set; }
        public decimal MontoRestante { get; set; }
    }

    public class CrearDeudaDto
    {
        [Required(ErrorMessage = "Escribe el nombre de la persona"), StringLength(80)]
        public string Persona     { get; set; } = string.Empty;
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;
        [Range(0.01, 9999999999.99, ErrorMessage = "El monto debe ser mayor a $0")]
        public decimal Monto      { get; set; }
        [Required, EnumDataType(typeof(TipoDeuda), ErrorMessage = "Tipo de deuda no válido")]
        public string Tipo        { get; set; } = string.Empty;
        public DateTime? FechaLimite { get; set; }
        public Guid? CuentaId     { get; set; }
    }

    public class AbonoDto
    {
        public Guid Id        { get; set; }
        public decimal Monto  { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class CrearAbonoDto
    {
        [Range(0.01, 9999999999.99, ErrorMessage = "El abono debe ser mayor a $0")]
        public decimal Monto { get; set; }
    }
}