namespace Finanzas.Application.DTOs
{
    public class CategoriaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activa { get; set; }
    }

    public class CrearCategoriaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
    }

    public class EditarCategoriaDto
    {
        public string Nombre { get; set; } = string.Empty;
    }

    public class ConteoCategoriasDto
    {
        public int Total { get; set; }
    }

    // Movimientos (gastos) de una categoría. Montos en pesos colombianos por ahora.
    public class CategoriaGastosDto
    {
        public Guid CategoriaId { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Moneda { get; set; } = "COP";
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
        public IEnumerable<GastoDto> Gastos { get; set; } = [];
    }

}
