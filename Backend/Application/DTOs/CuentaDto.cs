using System.ComponentModel.DataAnnotations;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.DTOs
{
    public class CuentaDto
    {
        public Guid Id       { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo   { get; set; } = string.Empty;
        public bool Activa   { get; set; }
        public decimal Saldo { get; set; }
        // true si el historial de la cuenta tiene al menos una línea (movimiento, ingreso o gasto)
        public bool TieneMovimientos { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime? ActualizadoEn { get; set; }
    }

    public class CrearCuentaDto
    {
        [Required(ErrorMessage = "Escribe un nombre"), StringLength(60)]
        public string Nombre { get; set; } = string.Empty;
        [Required, EnumDataType(typeof(TipoCuenta), ErrorMessage = "Tipo de cuenta no válido")]
        public string Tipo   { get; set; } = string.Empty;
        // Puede ser negativo (p. ej. una tarjeta de crédito con deuda)
        [Range(-9999999999.99, 9999999999.99)]
        public decimal SaldoInicial { get; set; }
    }

    public class EditarCuentaDto
    {
        [Required(ErrorMessage = "Escribe un nombre"), StringLength(60)]
        public string Nombre { get; set; } = string.Empty;
        [Required, EnumDataType(typeof(TipoCuenta), ErrorMessage = "Tipo de cuenta no válido")]
        public string Tipo   { get; set; } = string.Empty;
    }

    public class MovimientoCuentaDto
    {
        public Guid Id        { get; set; }
        public decimal Monto  { get; set; }
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class CrearMovimientoCuentaDto
    {
        // Firmado: positivo = depósito, negativo = retiro
        [Range(-9999999999.99, 9999999999.99)]
        public decimal Monto { get; set; }
        [StringLength(150)]
        public string? Descripcion { get; set; }
    }

    // Línea del historial unificado de una cuenta: movimientos manuales +
    // ingresos y gastos asignados a esa cuenta (todos afectan el saldo).
    public class LineaCuentaDto
    {
        public string Tipo { get; set; } = string.Empty; // MOVIMIENTO | INGRESO | GASTO
        public decimal Monto { get; set; }               // firmado: + suma al saldo, - resta
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
