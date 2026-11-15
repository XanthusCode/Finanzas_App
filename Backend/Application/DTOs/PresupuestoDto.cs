using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Selecciona una categoría"), StringLength(100)]
        public string Categoria { get; set; } = string.Empty;
        [Range(0.01, 9999999999.99, ErrorMessage = "El límite debe ser mayor a $0")]
        public decimal Limite { get; set; }
    }
}
