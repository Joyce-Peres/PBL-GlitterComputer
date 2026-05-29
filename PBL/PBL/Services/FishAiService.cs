using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PBL.Services
{
    public class FishAiResult
    {
        public string Especie { get; set; }
        public string NomeCientifico { get; set; }
        public decimal? TemperaturaIdeal { get; set; }
        public decimal? TemperaturaMin { get; set; }
        public decimal? TemperaturaMax { get; set; }
        public int? LuminosidadeIdeal { get; set; }
        public int? LuminosidadeMin { get; set; }
        public int? LuminosidadeMax { get; set; }
        public decimal? TdsPpmMin { get; set; }
        public decimal? TdsPpmMax { get; set; }
        public decimal? SalinidadePptMin { get; set; }
        public decimal? SalinidadePptMax { get; set; }
        public decimal? VolumeMinLitros { get; set; }

        public static FishAiResult FromJson(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            decimal? temp = null;
            if (root.TryGetProperty("dht22_temp_alvo", out var tempEl) && tempEl.ValueKind == JsonValueKind.Number)
                temp = tempEl.GetDecimal();

            int? luz = null;
            if (root.TryGetProperty("ldr_luz_alvo", out var luzEl) && luzEl.ValueKind == JsonValueKind.Number)
                luz = luzEl.GetInt32();

            decimal? tempMin = null;
            if (root.TryGetProperty("dht22_temp_min", out var tminEl) && tminEl.ValueKind == JsonValueKind.Number)
                tempMin = tminEl.GetDecimal();
            decimal? tempMax = null;
            if (root.TryGetProperty("dht22_temp_max", out var tmaxEl) && tmaxEl.ValueKind == JsonValueKind.Number)
                tempMax = tmaxEl.GetDecimal();

            int? luzMin = null;
            if (root.TryGetProperty("ldr_luz_min", out var lminEl) && lminEl.ValueKind == JsonValueKind.Number)
                luzMin = lminEl.GetInt32();
            int? luzMax = null;
            if (root.TryGetProperty("ldr_luz_max", out var lmaxEl) && lmaxEl.ValueKind == JsonValueKind.Number)
                luzMax = lmaxEl.GetInt32();

            decimal? tdsMin = null;
            if (root.TryGetProperty("tds_ppm_min", out var tdsMinEl) && tdsMinEl.ValueKind == JsonValueKind.Number)
                tdsMin = tdsMinEl.GetDecimal();
            decimal? tdsMax = null;
            if (root.TryGetProperty("tds_ppm_max", out var tdsMaxEl) && tdsMaxEl.ValueKind == JsonValueKind.Number)
                tdsMax = tdsMaxEl.GetDecimal();

            decimal? salMin = null;
            if (root.TryGetProperty("salinidade_ppt_min", out var salMinEl) && salMinEl.ValueKind == JsonValueKind.Number)
                salMin = salMinEl.GetDecimal();
            decimal? salMax = null;
            if (root.TryGetProperty("salinidade_ppt_max", out var salMaxEl) && salMaxEl.ValueKind == JsonValueKind.Number)
                salMax = salMaxEl.GetDecimal();

            decimal? volumeMin = null;
            if (root.TryGetProperty("volume_min_l", out var volumeMinEl) && volumeMinEl.ValueKind == JsonValueKind.Number)
                volumeMin = volumeMinEl.GetDecimal();

            return new FishAiResult
            {
                Especie = root.TryGetProperty("especie", out var espEl) ? espEl.GetString() : null,
                NomeCientifico = root.TryGetProperty("nome_cientifico", out var ncEl) ? ncEl.GetString() : null,
                TemperaturaIdeal = temp,
                LuminosidadeIdeal = luz,
                TemperaturaMin = tempMin,
                TemperaturaMax = tempMax,
                LuminosidadeMin = luzMin,
                LuminosidadeMax = luzMax,
                TdsPpmMin = tdsMin,
                TdsPpmMax = tdsMax,
                SalinidadePptMin = salMin,
                SalinidadePptMax = salMax,
                VolumeMinLitros = volumeMin
            };
        }
    }

    public class FishAiService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FishAiService> _logger;
        private readonly Client _client;
        private readonly string _model;

        public FishAiService(IConfiguration config, ILogger<FishAiService> logger)
        {
            _config = config;
            _logger = logger;

            // Tentar carregar o modelo da config, senão usa o padrão mais rápido (flash)
            // Gemini 2.5 flash é bem mais rápido para análise de imagens do que o pro
            _model = string.IsNullOrWhiteSpace(_config["FishAi:Model"]) ? "gemini-2.5-flash" : _config["FishAi:Model"];
            // Prioridade: env vars (.env) > env vars do sistema > appsettings
            // Assim dev pode usar .env no local sem tocar no appsettings.json
            var apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                ?? System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
                ?? System.Environment.GetEnvironmentVariable("FishAi__ApiKey")
                ?? _config["FishAi:ApiKey"];

            _client = string.IsNullOrWhiteSpace(apiKey) ? new Client() : new Client(apiKey: apiKey);
        }

        public async Task<FishAiResult> AnalisarImagemAsync(string caminhoImagem)
        {
            if (string.IsNullOrWhiteSpace(caminhoImagem) || !System.IO.File.Exists(caminhoImagem))
                throw new FileNotFoundException("Imagem não encontrada para análise.", caminhoImagem);

            try
            {
                // Calcula hash da imagem para rastreabilidade dos resultados
                // Se a API retorna resultado diferente com mesma imagem, fica fácil debugar
                string fileHash = null;
                try
                {
                    using var sha = SHA256.Create();
                    using var fs = System.IO.File.OpenRead(caminhoImagem);
                    var hash = sha.ComputeHash(fs);
                    fileHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    _logger?.LogInformation("Iniciando análise de imagem {Path} hash={Hash}", caminhoImagem, fileHash);
                }
                catch (Exception exHash)
                {
                    // Não falha se hash falhar - é só logging, não é crítico
                    _logger?.LogWarning(exHash, "Falha ao calcular hash da imagem");
                }

                var promptComando =
                    "Você é um especialista em peixes e aquários. Analise a imagem deste aquário doméstico. Identifique a espécie principal de peixe. " +
                    "Com base na literatura científica de aquarismo para essa espécie, defina os parâmetros ideais " +
                    "para o sensor DHT22 (temperatura alvo e faixa min/max), para o sensor LDR (luminosidade alvo e faixa min/max em escala de 0 a 100), " +
                    "para o sensor TDS/condutividade (faixa min/max em ppm), para salinidade em ppt, para o volume mínimo ideal do aquário em litros, " +
                    "Retorne estritamente um objeto JSON válido seguindo exatamente o modelo: " +
                    "{\"especie\": \"Nome\", \"nome_cientifico\": \"Nome\", \"dht22_temp_alvo\": 25.0, \"dht22_temp_min\": 24.0, \"dht22_temp_max\": 27.0, \"ldr_luz_alvo\": 40, \"ldr_luz_min\": 20, \"ldr_luz_max\": 60, \"tds_ppm_min\": 50.0, \"tds_ppm_max\": 150.0, \"salinidade_ppt_min\": 0.0, \"salinidade_ppt_max\": 1.0, \"volume_min_l\": 20.0}";

                var imageBytes = await System.IO.File.ReadAllBytesAsync(caminhoImagem);
                var content = new Content
                {
                    Role = "user",
                    Parts = new List<Part>
                    {
                        Part.FromText(promptComando),
                        Part.FromBytes(imageBytes, GetMimeType(caminhoImagem))
                    }
                };

                var response = await _client.Models.GenerateContentAsync(
                    model: _model,
                    contents: content,
                    config: new GenerateContentConfig { ResponseMimeType = "application/json" }
                );

                var json = ExtrairTextoResposta(response)?.Trim();
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger?.LogError("IA não retornou JSON para análise de imagem.");
                    throw new Exception("A IA não retornou JSON.");
                }

                _logger?.LogInformation("IA retornou JSON com {Length} bytes", json.Length);
                var result = FishAiResult.FromJson(json);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao executar análise de imagem");
                throw;
            }
        }

        public async Task<FishAiResult> AnalisarEspecieAsync(string especie)
        {
            if (string.IsNullOrWhiteSpace(especie))
                throw new Exception("Informe a espécie para análise.");

            try
            {
                _logger?.LogInformation("Analisando espécie pela IA: {Especie}", especie);

                var promptComando =
                    "Você é um assistente especialista em aquarismo. " +
                    "Com base na literatura científica e boas práticas de aquarismo, " +
                    "dada a espécie informada, defina parâmetros ideais para um aquário doméstico. " +
                    "Retorne estritamente um objeto JSON válido seguindo exatamente o modelo: " +
                    "{\"especie\": \"Nome\", \"nome_cientifico\": \"Nome\", \"dht22_temp_alvo\": 25.0, \"dht22_temp_min\": 24.0, \"dht22_temp_max\": 27.0, \"ldr_luz_alvo\": 40, \"ldr_luz_min\": 20, \"ldr_luz_max\": 60, \"tds_ppm_min\": 50.0, \"tds_ppm_max\": 150.0, \"salinidade_ppt_min\": 0.0, \"salinidade_ppt_max\": 1.0, \"volume_min_l\": 20.0}" +
                    "\n\nEspécie informada: " + especie.Trim();

                var response = await _client.Models.GenerateContentAsync(
                    model: _model,
                    contents: promptComando,
                    config: new GenerateContentConfig { ResponseMimeType = "application/json" }
                );

                var json = ExtrairTextoResposta(response)?.Trim();
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger?.LogError("IA não retornou JSON para espécie.");
                    throw new Exception("A IA não retornou JSON.");
                }

                _logger?.LogInformation("IA retornou JSON para espécie com {Length} bytes", json.Length);
                return FishAiResult.FromJson(json);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao analisar espécie");
                throw;
            }
        }

        private static string ExtrairTextoResposta(GenerateContentResponse response)
        {
            if (response == null)
                return null;

            if (!string.IsNullOrWhiteSpace(response.Text))
                return response.Text;

            if (response.Candidates == null)
                return null;

            foreach (var candidate in response.Candidates)
            {
                if (candidate?.Content?.Parts == null)
                    continue;

                foreach (var part in candidate.Content.Parts)
                {
                    if (!string.IsNullOrWhiteSpace(part?.Text))
                        return part.Text;
                }
            }

            return null;
        }

        private static string GetMimeType(string filePath)
        {
            var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
            switch (ext)
            {
                case ".png":
                    return "image/png";
                case ".webp":
                    return "image/webp";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".jpg":
                case ".jpeg":
                default:
                    return "image/jpeg";
            }
        }
    }
}
