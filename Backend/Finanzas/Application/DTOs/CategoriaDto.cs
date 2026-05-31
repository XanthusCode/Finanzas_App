namespace Finanzas.Application.DTOs
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool Activa { get; set; }
    }

    public class CrearCategoriaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
    }
}
