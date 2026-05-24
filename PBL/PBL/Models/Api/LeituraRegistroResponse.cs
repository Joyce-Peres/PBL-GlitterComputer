using System;

namespace PBL.Models.Api
{
    /// <summary>
    /// Resposta de confirmação após registrar uma leitura IoT.
    /// </summary>
    public class LeituraRegistroResponse
    {
        /// <summary>Mensagem de sucesso.</summary>
        /// <example>Leitura registrada com sucesso.</example>
        public string Mensagem { get; set; }

        /// <summary>Aquário associado à leitura.</summary>
        /// <example>1</example>
        public int AquarioId { get; set; }

        /// <summary>Data e hora do registro no servidor.</summary>
        public DateTime DataLeitura { get; set; }
    }
}
