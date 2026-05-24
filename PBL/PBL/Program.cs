using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace PBL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CarregarArquivoEnv();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void CarregarArquivoEnv()
        {
            var diretorioAtual = Directory.GetCurrentDirectory();
            var candidatos = new[]
            {
                Path.Combine(diretorioAtual, ".env"),
                Path.Combine(diretorioAtual, "..", ".env"),
                Path.Combine(diretorioAtual, "..", "..", ".env")
            };

            foreach (var caminho in candidatos)
            {
                var caminhoCompleto = Path.GetFullPath(caminho);
                if (!File.Exists(caminhoCompleto))
                    continue;

                foreach (var linha in File.ReadAllLines(caminhoCompleto))
                {
                    var texto = linha.Trim();
                    if (texto.Length == 0 || texto.StartsWith("#"))
                        continue;

                    var separador = texto.IndexOf('=');
                    if (separador <= 0)
                        continue;

                    var chave = texto.Substring(0, separador).Trim();
                    var valor = texto.Substring(separador + 1).Trim().Trim('"');
                    if (chave.Length > 0)
                        Environment.SetEnvironmentVariable(chave, valor);
                }

                break;
            }
        }
    }
}
