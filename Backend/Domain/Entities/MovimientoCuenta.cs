namespace Finanzas.Domain.Entities
{
    public class MovimientoCuenta
    {
        public Guid Id          { get; set; } = Guid.NewGuid();
        public Guid CuentaId    { get; set; }
        public decimal Monto    { get; set; } // positivo = depósito, negativo = retiro
        public string? Descripcion { get; set; }
        public Guid? DeudaId    { get; set; } // si el movimiento nace de un préstamo
        public DateTime Fecha   { get; set; } = DateTime.UtcNow;
    }
}
