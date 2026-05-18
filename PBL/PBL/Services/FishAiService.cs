using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PBL.Services
{
    public class FishAiResult
    {
        public string Especie { get; set; }
        public string NomeCientifico { get; set; }
        public decimal? TemperaturaIdeal { get; set; }
        public int? LuminosidadeIdeal { get; set; }

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

            return new FishAiResult
            {
                Especie = root.TryGetProperty("especie", out var espEl) ? espEl.GetString() : null,
                NomeCientifico = root.TryGetProperty("nome_cientifico", out var ncEl) ? ncEl.GetString() : null,
                TemperaturaIdeal = temp,
                LuminosidadeIdeal = luz
            };
        }
    }

    public class FishAiService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public FishAiService(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public async Task<FishAiResult> AnalisarImagemAsync(string caminhoImagem)
        {
            if (string.IsNullOrWhiteSpace(caminhoImagem) || !File.Exists(caminhoImagem))
                throw new FileNotFoundException("Imagem não encontrada para análise.", caminhoImagem);

            var pythonExe = _config["FishAi:PythonExe"];
            if (string.IsNullOrWhiteSpace(pythonExe))
                pythonExe = "python";

            var scriptRel = _config["FishAi:ScriptPath"];
            if (string.IsNullOrWhiteSpace(scriptRel))
                scriptRel = Path.Combine("cadastro-peixe", "reconhecer_peixe.py");

            var scriptAbs = Path.GetFullPath(Path.Combine(_env.ContentRootPath, scriptRel));
            if (!File.Exists(scriptAbs))
                throw new FileNotFoundException("Script de IA não encontrado.", scriptAbs);

            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptAbs}\" \"{caminhoImagem}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            // Permite configurar a chave da IA via variável de ambiente.
            // Ex.: GEMINI_API_KEY=... (não deve ficar hardcoded em código/fonte)
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (!string.IsNullOrWhiteSpace(apiKey))
                psi.Environment["GEMINI_API_KEY"] = apiKey;

            using var proc = new Process { StartInfo = psi };
            proc.Start();

            var stdout = await proc.StandardOutput.ReadToEndAsync();
            var stderr = await proc.StandardError.ReadToEndAsync();

            await Task.Run(() => proc.WaitForExit());
            if (proc.ExitCode != 0)
                throw new Exception(string.IsNullOrWhiteSpace(stderr) ? "Falha ao executar a IA." : stderr);

            var json = stdout?.Trim();
            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("A IA não retornou JSON.");

            return FishAiResult.FromJson(json);
        }

        public async Task<FishAiResult> AnalisarEspecieAsync(string especie)
        {
            if (string.IsNullOrWhiteSpace(especie))
                throw new Exception("Informe a espécie para análise.");

            var pythonExe = _config["FishAi:PythonExe"];
            if (string.IsNullOrWhiteSpace(pythonExe))
                pythonExe = "python";

            var scriptRel = _config["FishAi:ScriptPath"];
            if (string.IsNullOrWhiteSpace(scriptRel))
                scriptRel = Path.Combine("cadastro-peixe", "reconhecer_peixe.py");

            var scriptAbs = Path.GetFullPath(Path.Combine(_env.ContentRootPath, scriptRel));
            if (!File.Exists(scriptAbs))
                throw new FileNotFoundException("Script de IA não encontrado.", scriptAbs);

            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptAbs}\" --species \"{especie.Trim()}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (!string.IsNullOrWhiteSpace(apiKey))
                psi.Environment["GEMINI_API_KEY"] = apiKey;

            using var proc = new Process { StartInfo = psi };
            proc.Start();

            var stdout = await proc.StandardOutput.ReadToEndAsync();
            var stderr = await proc.StandardError.ReadToEndAsync();

            await Task.Run(() => proc.WaitForExit());
            if (proc.ExitCode != 0)
                throw new Exception(string.IsNullOrWhiteSpace(stderr) ? "Falha ao executar a IA." : stderr);

            var json = stdout?.Trim();
            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("A IA não retornou JSON.");

            return FishAiResult.FromJson(json);
        }
    }
}
