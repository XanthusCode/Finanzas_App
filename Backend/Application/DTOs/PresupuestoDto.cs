namespace Finanzas.Application.DTOs
{
    public class PresupuestoDto
    {
        public Guid Id { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public decimal Limite { get; set; }
    }

    public class CrearPresupuestoDto
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Limite { get; set; }
    }
}
