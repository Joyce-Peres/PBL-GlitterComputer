using System;

namespace PBL.Models
{
    public class SmartLampConfigViewModel
    {
        public int AquarioId { get; set; }
        public string NomeAquario { get; set; }

        public int Modo { get; set; } = 4;
        public int Brilho { get; set; } = 80;

        public int R { get; set; } = 255;
        public int G { get; set; } = 255;
        public int B { get; set; } = 255;

        public int? LuzAlvo { get; set; }
        public decimal? TempAlvo { get; set; }

        public DateTime? AtualizadoEm { get; set; }
    }
}
