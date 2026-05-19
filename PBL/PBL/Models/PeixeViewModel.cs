namespace PBL.Models
{
    public class PeixeViewModel : PadraoViewModel
    {
        public string Nome { get; set; }
        public string Especie { get; set; }
        public string NomeCientifico { get; set; }
        public decimal TamanhoCm { get; set; }
        public int AquarioId { get; set; }
        public string Foto { get; set; }
        public string NomeAquario { get; set; }

        /// <summary>
        /// Parâmetros ambientais recomendados para a espécie
        /// </summary>
        public FishParametersViewModel Parameters { get; set; } = new FishParametersViewModel();

        // Propriedades de compatibilidade/mapeamento direto para a DAO
        public decimal? TemperaturaIdeal
        {
            get => Parameters?.TemperaturaIdeal;
            set => Parameters.TemperaturaIdeal = value;
        }

        public decimal? TemperaturaMin
        {
            get => Parameters?.TemperaturaMin;
            set => Parameters.TemperaturaMin = value;
        }

        public decimal? TemperaturaMax
        {
            get => Parameters?.TemperaturaMax;
            set => Parameters.TemperaturaMax = value;
        }

        public int? LuminosidadeIdeal
        {
            get => Parameters?.LuminosidadeIdeal;
            set => Parameters.LuminosidadeIdeal = value;
        }

        public int? LuminosidadeMin
        {
            get => Parameters?.LuminosidadeMin;
            set => Parameters.LuminosidadeMin = value;
        }

        public int? LuminosidadeMax
        {
            get => Parameters?.LuminosidadeMax;
            set => Parameters.LuminosidadeMax = value;
        }

        public decimal? PhMin
        {
            get => Parameters?.PhMin;
            set => Parameters.PhMin = value;
        }

        public decimal? PhMax
        {
            get => Parameters?.PhMax;
            set => Parameters.PhMax = value;
        }

        public bool? OriginFromAI
        {
            get => Parameters?.OriginFromAI;
            set => Parameters.OriginFromAI = value;
        }
    }
}
