namespace Finanzas.Domain.Entities
{
   public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public TipoGasto Tipo { get; set; }
        public bool Activa { get; set; } = true;
    }
}
