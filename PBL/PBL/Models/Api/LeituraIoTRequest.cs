using System.ComponentModel.DataAnnotations;

namespace PBL.Models.Api
{
    /// <summary>
    /// Payload enviado pelo dispositivo IoT para registrar uma leitura de sensores.
    /// </summary>
    public class LeituraIoTRequest
    {
        /// <summary>Identificador do aquário (FK em Aquarios).</summary>
        /// <example>1</example>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um aquarioId válido.")]
        public int AquarioId { get; set; }

        /// <summary>Temperatura da água em °C (sensor DHT22 ou equivalente).</summary>
        /// <example>25.5</example>
        [Required]
        [Range(-10, 60)]
        public decimal Temperatura { get; set; }

        /// <summary>Nível de água em percentual (0 a 100).</summary>
        /// <example>85</example>
        [Required]
        [Range(0, 100)]
        public decimal NivelAgua { get; set; }

        /// <summary>TDS da água em ppm.</summary>
        /// <example>120</example>
        [Range(0, double.MaxValue)]
        public decimal? TdsPpm { get; set; }

        /// <summary>Salinidade em ppt.</summary>
        /// <example>1.85</example>
        [Range(0, double.MaxValue)]
        public decimal? SalinidadePpt { get; set; }

        /// <summary>Qualidade textual já classificada pelo simulador.</summary>
        /// <example>Boa</example>
        [StringLength(100)]
        public string QualidadeTds { get; set; }
    }
}
