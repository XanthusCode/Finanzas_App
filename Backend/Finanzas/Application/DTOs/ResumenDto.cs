namespace Finanzas.Application.DTOs
{
    public class ResumenDto
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalFijos { get; set; }
        public decimal TotalVariables { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Ahorro { get; set; }
        public int PctAhorro { get; set; }
    }
}
