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
        public bool Pagada      { get; set; }
        public DateTime CreadoEn { get; set; }
        public decimal MontoPagado   { get; set; }
        public decimal MontoRestante { get; set; }
    }

    public class CrearDeudaDto
    {
        public string Persona     { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto      { get; set; }
        public string Tipo        { get; set; } = string.Empty;
        public DateTime? FechaLimite { get; set; }
    }

    public class AbonoDto
    {
        public Guid Id        { get; set; }
        public decimal Monto  { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class CrearAbonoDto
    {
        public decimal Monto { get; set; }
    }
}