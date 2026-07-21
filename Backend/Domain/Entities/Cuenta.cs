namespace Finanzas.Domain.Entities
{
    public enum TipoCuenta { EFECTIVO, BANCO, BILLETERA_DIGITAL, TARJETA_CREDITO, ALCANCIA, CRIPTO, OTRO }

    public class Cuenta
    {
        public Guid Id       { get; set; } = Guid.NewGuid();
        public Guid UserId   { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public TipoCuenta Tipo { get; set; }
        public bool Activa   { get; set; } = true;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}
