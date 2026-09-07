namespace Finanzas.Domain.Entities
{
    // Entidades que registran la fecha de su última modificación.
    // AppDbContext.SaveChanges sella ActualizadoEn automáticamente.
    public interface IAuditable
    {
        DateTime? ActualizadoEn { get; set; }
    }
}
