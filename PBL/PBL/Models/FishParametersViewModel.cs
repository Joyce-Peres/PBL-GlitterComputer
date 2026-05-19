namespace PBL.Models
{
    public class FishParametersViewModel
    {
        /// <summary>
        /// Temperatura ideal em °C
        /// </summary>
        public decimal? TemperaturaIdeal { get; set; }

        /// <summary>
        /// Temperatura mínima recomendada em °C
        /// </summary>
        public decimal? TemperaturaMin { get; set; }

        /// <summary>
        /// Temperatura máxima recomendada em °C
        /// </summary>
        public decimal? TemperaturaMax { get; set; }

        /// <summary>
        /// Luminosidade ideal (escala 0-100)
        /// </summary>
        public int? LuminosidadeIdeal { get; set; }

        /// <summary>
        /// Luminosidade mínima (escala 0-100)
        /// </summary>
        public int? LuminosidadeMin { get; set; }

        /// <summary>
        /// Luminosidade máxima (escala 0-100)
        /// </summary>
        public int? LuminosidadeMax { get; set; }

        /// <summary>
        /// pH mínimo
        /// </summary>
        public decimal? PhMin { get; set; }

        /// <summary>
        /// pH máximo
        /// </summary>
        public decimal? PhMax { get; set; }

        /// <summary>
        /// Indica se os parâmetros foram gerados pela IA (true) ou digitados manualmente (false)
        /// </summary>
        public bool? OriginFromAI { get; set; }

        /// <summary>
        /// Data/hora em que os parâmetros foram definidos ou atualizados
        /// </summary>
        public System.DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Valida se os parâmetros têm coerência (min <= ideal <= max, ph_min <= ph_max, etc)
        /// </summary>
        public bool IsValid(out string mensagemErro)
        {
            mensagemErro = null;

            // Validar temperatura
            if (TemperaturaMin.HasValue && TemperaturaIdeal.HasValue && TemperaturaMin > TemperaturaIdeal)
            {
                mensagemErro = "Temperatura mínima não pode ser maior que a ideal.";
                return false;
            }
            if (TemperaturaIdeal.HasValue && TemperaturaMax.HasValue && TemperaturaIdeal > TemperaturaMax)
            {
                mensagemErro = "Temperatura ideal não pode ser maior que a máxima.";
                return false;
            }
            if (TemperaturaMin.HasValue && TemperaturaMax.HasValue && TemperaturaMin > TemperaturaMax)
            {
                mensagemErro = "Temperatura mínima não pode ser maior que a máxima.";
                return false;
            }

            // Validar luminosidade
            if (LuminosidadeMin.HasValue && LuminosidadeIdeal.HasValue && LuminosidadeMin > LuminosidadeIdeal)
            {
                mensagemErro = "Luminosidade mínima não pode ser maior que a ideal.";
                return false;
            }
            if (LuminosidadeIdeal.HasValue && LuminosidadeMax.HasValue && LuminosidadeIdeal > LuminosidadeMax)
            {
                mensagemErro = "Luminosidade ideal não pode ser maior que a máxima.";
                return false;
            }
            if (LuminosidadeMin.HasValue && LuminosidadeMax.HasValue && LuminosidadeMin > LuminosidadeMax)
            {
                mensagemErro = "Luminosidade mínima não pode ser maior que a máxima.";
                return false;
            }

            // Validar pH
            if (PhMin.HasValue && PhMax.HasValue && PhMin > PhMax)
            {
                mensagemErro = "pH mínimo não pode ser maior que o máximo.";
                return false;
            }

            return true;
        }
    }
}
