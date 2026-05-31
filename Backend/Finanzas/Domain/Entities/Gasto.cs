namespace Finanzas.Domain.Entities
{
    public class Gasto
    {
        public int Id { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public TipoGasto Tipo { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }

    public enum TipoGasto
    {
        FIJO,
        VARIABLE
    }
}
