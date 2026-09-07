namespace Finanzas.Domain.Entities
{
    public enum TipoDeuda { ME_DEBEN, LE_DEBO }

    public class Deuda : IAuditable
    {
        public Guid Id          { get; set; } = Guid.NewGuid();
        public Guid UserId      { get; set; }
        public string Persona   { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto    { get; set; }
        public TipoDeuda Tipo   { get; set; }
        public DateTime? FechaLimite { get; set; }
        public Guid? CuentaId   { get; set; } // cuenta de la que sale / entra el préstamo
        public bool Pagada      { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? ActualizadoEn { get; set; }
    }
}
