namespace Finanzas.Application.DTOs
{
    public class CuentaDto
    {
        public Guid Id       { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo   { get; set; } = string.Empty;
        public bool Activa   { get; set; }
        public decimal Saldo { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime? ActualizadoEn { get; set; }
    }

    public class CrearCuentaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Tipo   { get; set; } = string.Empty;
        public decimal SaldoInicial { get; set; }
    }

    public class EditarCuentaDto
    {
        public string Nombre { get; set; } = string.Empty;
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
        public decimal Monto { get; set; }
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
