namespace Finanzas.Application.DTOs
{
    public class IngresoDto
    {
        public Guid Id { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
    }

    public class CrearIngresoDto
    {
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
    }
}
