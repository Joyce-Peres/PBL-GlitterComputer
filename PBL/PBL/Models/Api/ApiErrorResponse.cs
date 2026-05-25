namespace PBL.Models.Api
{
    /// <summary>
    /// Resposta padrão de erro da API.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>Descrição do erro.</summary>
        /// <example>Dados inválidos. Informe aquarioId, temperatura e nivelAgua.</example>
        public string Erro { get; set; }
    }
}
