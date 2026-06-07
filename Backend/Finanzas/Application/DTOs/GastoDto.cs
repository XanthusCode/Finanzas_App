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
    }

    public class CrearGastoDto
    {
        public string Categoria { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int Mes { get; set; }
        public int Anio { get; set; }
    }
}
