namespace Finanzas.Domain.Entities
{
   public class Categoria
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public Guid UserId { get; set; }
    }
}
