namespace PBL.Models
{
    public class PeixeViewModel : PadraoViewModel
    {
        public string Nome { get; set; }
        public string Especie { get; set; }
        public string NomeCientifico { get; set; }
        public decimal? TemperaturaIdeal { get; set; }
        public decimal? TemperaturaMin { get; set; }
        public decimal? TemperaturaMax { get; set; }
        public int? LuminosidadeIdeal { get; set; }
        public int? LuminosidadeMin { get; set; }
        public int? LuminosidadeMax { get; set; }
        public decimal TamanhoCm { get; set; }
        public decimal? PhMin { get; set; }
        public decimal? PhMax { get; set; }
        public int AquarioId { get; set; }
        public string Foto { get; set; }
        public string NomeAquario { get; set; }
    }
}
