namespace Finanzas.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Opt-out de todos los emails de RecordatoriosService.
        public bool RecordatoriosActivos { get; set; } = true;
        // Último email de "registra tus gastos / cobros por confirmar"; evita mandarlo todos los días.
        public DateTime? UltimoRecordatorioActividadEn { get; set; }

        // Valor de "From" que manda Twilio ("whatsapp:+57..." o, si el usuario tiene
        // nombre de usuario en WhatsApp, un ID opaco "whatsapp:CO.123..."). Por eso se
        // vincula con un código enviado desde WhatsApp y no escribiendo el teléfono.
        public string? WhatsAppId { get; set; }
        public string? WhatsAppCodigo { get; set; }
        public DateTime? WhatsAppCodigoExpira { get; set; }
    }
}
