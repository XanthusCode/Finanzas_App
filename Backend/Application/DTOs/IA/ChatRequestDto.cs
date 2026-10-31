using System;
using System.Collections.Generic;
using System.Text;

namespace Finanzas.Application.DTOs.IA
{
    public class ChatRequestDto
    {
        public List<ChatMensajeDto> Mensajes { get; set; } = [];
    }
}
