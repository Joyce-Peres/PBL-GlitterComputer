namespace PBL.Models
{
    public class PeixeViewModel : PadraoViewModel
    {
        public string Nome { get; set; }
        public string Especie { get; set; }
        public decimal TamanhoCm { get; set; }
        public int AquarioId { get; set; }
        public string Foto { get; set; }
        public string NomeAquario { get; set; }
    }
}
