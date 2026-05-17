using System;

namespace PBL.Models
{
    public class LeituraSensorViewModel : PadraoViewModel
    {
        public int AquarioId { get; set; }
        public decimal Temperatura { get; set; }
        public decimal Ph { get; set; }
        public decimal NivelAgua { get; set; }
        public DateTime DataLeitura { get; set; }
        public string NomeAquario { get; set; }
    }
}
