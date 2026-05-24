using System;

namespace PBL.Models
{
    /// <summary>
    /// Leitura de sensores associada a um aquário (resposta dos endpoints GET).
    /// </summary>
    public class LeituraSensorViewModel : PadraoViewModel
    {
        /// <summary>Identificador do aquário.</summary>
        public int AquarioId { get; set; }

        /// <summary>Temperatura principal em °C.</summary>
        public decimal Temperatura { get; set; }
        public decimal? TemperaturaAgua { get; set; }
        public decimal? TemperaturaAr { get; set; }
        public decimal? UmidadeAr { get; set; }
        public decimal? TdsPpm { get; set; }
        public int? LdrRaw { get; set; }
        public decimal? DistanciaAguaCm { get; set; }
        public decimal? NivelPct { get; set; }
        public decimal? VolumeLitros { get; set; }
        /// <summary>pH da água.</summary>
        public decimal Ph { get; set; }

        /// <summary>Nível de água (percentual ou valor legado).</summary>
        public decimal NivelAgua { get; set; }

        /// <summary>Data e hora da leitura.</summary>
        public DateTime DataLeitura { get; set; }

        /// <summary>Origem: STH-Comet, SQL/Legado, etc.</summary>
        public string FonteDados { get; set; }

        /// <summary>Classificação da qualidade da água (quando disponível).</summary>
        public string QualidadeAgua { get; set; }

        /// <summary>Nome do aquário (join).</summary>
        public string NomeAquario { get; set; }
    }
}
