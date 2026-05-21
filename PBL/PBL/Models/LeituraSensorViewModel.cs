using System;

namespace PBL.Models
{
    public class LeituraSensorViewModel : PadraoViewModel
    {
        public int AquarioId { get; set; }
        public decimal Temperatura { get; set; }
        public decimal? TemperaturaAgua { get; set; }
        public decimal? TemperaturaAr { get; set; }
        public decimal? UmidadeAr { get; set; }
        public decimal? TdsPpm { get; set; }
        public int? LdrRaw { get; set; }
        public decimal? DistanciaAguaCm { get; set; }
        public decimal? NivelPct { get; set; }
        public decimal? VolumeLitros { get; set; }
        public decimal Ph { get; set; }
        public decimal NivelAgua { get; set; }
        public DateTime DataLeitura { get; set; }
        public string FonteDados { get; set; }
        public string QualidadeAgua { get; set; }
        public string NomeAquario { get; set; }
    }
}
