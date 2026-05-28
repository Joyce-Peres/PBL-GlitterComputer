using Microsoft.Extensions.Configuration;
using PBL.DAO;
using PBL.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PBL.Services
{
    // Serviço responsável por buscar e transformar o histórico de sensores vindo do FIWARE/STH-Comet.
    public class FiwareSthCometService
    {
        // Cliente HTTP reutilizado para consultar o STH-Comet com timeout curto.
        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };

        private readonly IConfiguration _config;

        public FiwareSthCometService(IConfiguration config)
        {
            _config = config;
        }

        // Indica se a integração FIWARE está pronta para uso.
        public bool EstaConfigurado
        {
            get
            {
                return !string.IsNullOrWhiteSpace(GetBaseUrl())
                    && !string.IsNullOrWhiteSpace(GetEntityType())
                    && !string.IsNullOrWhiteSpace(GetEntityId());
            }
        }

        public async Task<List<LeituraSensorViewModel>> ConsultarHistoricoAsync(
            int? aquarioId,
            DateTime? dataInicio,
            DateTime? dataFim,
            int? lastN = null)
        {
            if (!EstaConfigurado)
                return new List<LeituraSensorViewModel>();

            var fim = dataFim.HasValue ? dataFim.Value : DateTime.UtcNow;
            var inicio = dataInicio.HasValue ? dataInicio.Value : fim.AddHours(-24);

            if (fim <= inicio)
                fim = inicio.AddHours(1);

            // Resolve qual entidade FIWARE pertence ao aquário selecionado.
            // Ex.: Aquario.Id = 1 -> FiwareEntityId = Thing:lamp001.
            // Se nenhum aquário for selecionado, usa o EntityId padrão do appsettings.
            var contextoAquario = ResolverContextoAquario(aquarioId);

            // Descobre qual entidade FIWARE representa o aquário selecionado.
            if (string.IsNullOrWhiteSpace(contextoAquario.EntityId))
                return new List<LeituraSensorViewModel>();

            // Atributos realmente gravados pela subscription do STH-Comet.
            // Consultar apenas estes evita chamadas vazias e deixa o dashboard mais rápido.
            // Atributos históricos que o dashboard realmente precisa exibir.
            var atributos = new[]
            {
                "temp_agua",
                "temp_ar",
                "umidade_ar",
                "tds_ppm",
                "salinidade_ppt",
                "qualidade_agua",
                "ec_us_cm",
                "ldr_raw",
                "dist_agua_cm",
                "nivel_pct",
                "volume_litros"
            };

            var tarefas = atributos
                .Select(atributo => ConsultarSerieAtributoAsync(atributo, contextoAquario.EntityId, inicio, fim, lastN))
                .ToArray();

            await Task.WhenAll(tarefas);

            var pontos = tarefas
                .SelectMany(tarefa => tarefa.Result)
                .ToList();

            // Junta os valores por instante para devolver uma lista de leituras pronta para a tela.
            return MesclarPontos(pontos, contextoAquario);
        }

        // Consulta uma série histórica de um atributo específico no STH-Comet.
        private async Task<List<PontoHistorico>> ConsultarSerieAtributoAsync(
            string atributo,
            string entityId,
            DateTime inicio,
            DateTime fim,
            int? lastN)
        {
            var url = MontarUrlConsulta(atributo, entityId, inicio, fim, lastN);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Prepara a requisição HTTP e inclui os headers FIWARE quando configurados.
            // Adiciona headers Fiware se configurados (muitos deploys exigem 'fiware-service')
            var fiwareService = _config["FiwareSthComet:FiwareService"];
            if (!string.IsNullOrWhiteSpace(fiwareService))
                request.Headers.Add("fiware-service", fiwareService);

            var fiwareServicePath = _config["FiwareSthComet:FiwareServicePath"];
            if (!string.IsNullOrWhiteSpace(fiwareServicePath))
                request.Headers.Add("fiware-servicepath", fiwareServicePath);

            try
            {
                var resposta = await HttpClient.SendAsync(request);

                if (!resposta.IsSuccessStatusCode)
                    return new List<PontoHistorico>();

                var json = await resposta.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json))
                    return new List<PontoHistorico>();

                using (var doc = JsonDocument.Parse(json))
                {
                    return ExtrairPontos(doc.RootElement, atributo);
                }
            }
            catch
            {
                // Ex.: rede da faculdade bloqueando a porta 8666 ou EC2 desligada.
                // Retorna lista vazia para o dashboard não quebrar a página inteira.
                return new List<PontoHistorico>();
            }
        }
        // Monta a URL exata usada para consultar o histórico do atributo no STH-Comet.

        private string MontarUrlConsulta(string atributo, string entityId, DateTime inicio, DateTime fim, int? lastN)
        {
            var baseUrl = GetBaseUrl().TrimEnd('/');
            var entityType = Uri.EscapeDataString(GetEntityType().Trim());
            var entityIdUrl = Uri.EscapeDataString(entityId.Trim());
            var attr = Uri.EscapeDataString(atributo.Trim());
            // O projeto usa lastN porque essa consulta se mostrou mais confiável do que fromDate/toDate.

            // O STH-Comet do projeto está retornando corretamente com lastN.
            // fromDate/toDate estava fazendo o dashboard retornar "Nenhuma leitura encontrada".
            var quantidade = lastN.HasValue && lastN.Value > 0 ? lastN.Value : 20;

            // Extrai os pontos históricos do JSON retornado pelo STH-Comet.
            return $"{baseUrl}/STH/v1/contextEntities/type/{entityType}/id/{entityIdUrl}/attributes/{attr}?lastN={quantidade}";
        }

        private List<PontoHistorico> ExtrairPontos(JsonElement root, string atributo)
        {
            var pontos = new List<PontoHistorico>();

            if (!root.TryGetProperty("contextResponses", out var contextResponses) || contextResponses.ValueKind != JsonValueKind.Array)
                return pontos;

            foreach (var contexto in contextResponses.EnumerateArray())
            {
                if (!TentaObterPropriedade(contexto, "contextElement", out var contextElement))
                    continue;

                if (!TentaObterPropriedade(contextElement, "attributes", out var attributes) || attributes.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var attr in attributes.EnumerateArray())
                {
                    var nomeAtributo = ObterTexto(attr, "name");
                    if (!string.Equals(nomeAtributo, atributo, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (TentaObterPropriedade(attr, "values", out var values) && values.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var value in values.EnumerateArray())
                            AdicionarPonto(pontos, atributo, value);
                    }
                    else
                    {
                        AdicionarPonto(pontos, atributo, attr);
                    }
                }
            }

            return pontos;
        }

        private void AdicionarPonto(List<PontoHistorico> pontos, string atributo, JsonElement elemento)
        {
            var valor = ObterDecimal(elemento, "attrValue");
            if (!valor.HasValue)
                valor = ObterDecimal(elemento, "value");

            var texto = ObterTexto(elemento, "attrValue") ?? ObterTexto(elemento, "value");
            if (string.IsNullOrWhiteSpace(texto) && elemento.ValueKind == JsonValueKind.String)
                texto = elemento.GetString();

            if (!valor.HasValue && elemento.ValueKind == JsonValueKind.String)
            {
                decimal textoDecimal;
                if (decimal.TryParse(elemento.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out textoDecimal))
                    valor = textoDecimal;
            }

            var data = ObterDataHora(elemento);
            if (!data.HasValue)
                return;

            pontos.Add(new PontoHistorico
            {
                Atributo = atributo,
                Valor = valor,
                Texto = texto,
                DataLeituraUtc = DateTime.SpecifyKind(data.Value, DateTimeKind.Utc)
                // Agrupa os pontos por instante de leitura e monta a lista final exibida no dashboard.
            });
        }

        private List<LeituraSensorViewModel> MesclarPontos(List<PontoHistorico> pontos, ContextoAquario contextoAquario)
        {
            var linhas = new Dictionary<long, LeituraSensorViewModel>();
            var nomeAquario = contextoAquario.NomeAquario;
            var aquarioFinal = contextoAquario.AquarioId;

            foreach (var ponto in pontos.OrderBy(p => p.DataLeituraUtc))
            {
                var chave = ponto.DataLeituraUtc.ToUniversalTime().Ticks;
                LeituraSensorViewModel leitura;

                if (!linhas.TryGetValue(chave, out leitura))
                {
                    leitura = new LeituraSensorViewModel
                    {
                        Id = linhas.Count + 1,
                        AquarioId = aquarioFinal,
                        NomeAquario = nomeAquario,
                        DataLeitura = ponto.DataLeituraUtc.ToLocalTime(),
                        FonteDados = "MongoDB via STH-Comet"
                    };
                    linhas.Add(chave, leitura);
                }

                switch (ponto.Atributo)
                {
                    case "temp_agua":
                        leitura.TemperaturaAgua = ponto.Valor;
                        if (ponto.Valor.HasValue)
                            leitura.Temperatura = ponto.Valor.Value;
                        break;
                    case "temp_ar":
                        leitura.TemperaturaAr = ponto.Valor;
                        break;
                    case "umidade_ar":
                        leitura.UmidadeAr = ponto.Valor;
                        break;
                    case "ec_us_cm":
                    case "tds_ppm":
                        leitura.TdsPpm = ponto.Valor;
                        break;
                    case "qualidade_agua":
                    case "qualidade_tds":
                        leitura.QualidadeAgua = ponto.Texto;
                        break;
                    case "alerta":
                        leitura.Alerta = ponto.Texto;
                        break;
                    case "salinidade_ppt":
                        leitura.SalinidadePpt = ponto.Valor;
                        if (!leitura.SalinidadePpt.HasValue && !string.IsNullOrWhiteSpace(ponto.Texto))
                        {
                            decimal salinidade;
                            if (decimal.TryParse(ponto.Texto, NumberStyles.Any, CultureInfo.InvariantCulture, out salinidade))
                                leitura.SalinidadePpt = salinidade;
                        }
                        break;
                    case "ldr_raw":
                        leitura.LdrRaw = ponto.Valor.HasValue ? (int?)Convert.ToInt32(ponto.Valor.Value) : null;
                        break;
                    case "dist_agua_cm":
                        leitura.DistanciaAguaCm = ponto.Valor;
                        break;
                    case "nivel_pct":
                        leitura.NivelPct = ponto.Valor;
                        if (ponto.Valor.HasValue)
                            leitura.NivelAgua = ponto.Valor.Value;
                        break;
                    case "volume_litros":
                        leitura.VolumeLitros = ponto.Valor;
                        break;
                }
            }

            return linhas.Values
                .OrderByDescending(item => item.DataLeitura)
                // Resolve o aquário e a entidade FIWARE que devem ser usados na consulta.
                .ToList();
        }

        private ContextoAquario ResolverContextoAquario(int? aquarioId)
        {
            var entityIdPadrao = GetEntityId();
            var nomePadrao = GetNomeEntidade();
            var aquarioIdPadrao = GetInt("FiwareSthComet:DefaultAquarioId", 1);

            try
            {
                var aquarioDao = new AquarioDAO();

                if (aquarioId.HasValue && aquarioId.Value > 0)
                {
                    var aquario = aquarioDao.Consulta(aquarioId.Value);
                    if (aquario != null)
                    {
                        var entityIdDoAquario = !string.IsNullOrWhiteSpace(aquario.FiwareEntityId)
                            ? aquario.FiwareEntityId.Trim()
                            : (aquario.Id == aquarioIdPadrao ? entityIdPadrao : "");

                        return new ContextoAquario
                        {
                            AquarioId = aquario.Id,
                            NomeAquario = !string.IsNullOrWhiteSpace(aquario.Nome) ? aquario.Nome : nomePadrao,
                            EntityId = entityIdDoAquario
                        };
                    }
                }

                // Sem filtro: tenta descobrir o nome amigável pelo EntityId padrão.
                var aquarioPorEntidade = aquarioDao.ConsultaPorFiwareEntityId(entityIdPadrao);
                if (aquarioPorEntidade != null)
                {
                    return new ContextoAquario
                    {
                        AquarioId = aquarioPorEntidade.Id,
                        NomeAquario = !string.IsNullOrWhiteSpace(aquarioPorEntidade.Nome) ? aquarioPorEntidade.Nome : nomePadrao,
                        EntityId = entityIdPadrao
                    };
                }

                // Fallback: usa DefaultAquarioId do appsettings.
                var aquarioPadrao = aquarioDao.Consulta(aquarioIdPadrao);
                if (aquarioPadrao != null)
                {
                    return new ContextoAquario
                    {
                        AquarioId = aquarioPadrao.Id,
                        NomeAquario = !string.IsNullOrWhiteSpace(aquarioPadrao.Nome) ? aquarioPadrao.Nome : nomePadrao,
                        EntityId = !string.IsNullOrWhiteSpace(aquarioPadrao.FiwareEntityId) ? aquarioPadrao.FiwareEntityId.Trim() : entityIdPadrao
                    };
                }
            }
            catch
            {
                // Caso o SQL esteja indisponível, mantém o funcionamento via appsettings.
            }

            return new ContextoAquario
            {
                AquarioId = aquarioId.HasValue && aquarioId.Value > 0 ? aquarioId.Value : aquarioIdPadrao,
                NomeAquario = nomePadrao,
                EntityId = entityIdPadrao
            };
        }

        private string GetBaseUrl()
        {
            return _config["FiwareSthComet:BaseUrl"] ?? "";
        }

        private string GetEntityType()
        {
            return _config["FiwareSthComet:EntityType"] ?? "";
        }

        private string GetEntityId()
        {
            return _config["FiwareSthComet:EntityId"] ?? "";
        }

        private string GetNomeEntidade()
        {
            var nome = _config["FiwareSthComet:EntityLabel"];
            if (!string.IsNullOrWhiteSpace(nome))
                return nome;

            return GetEntityId();
        }

        private int GetInt(string key, int defaultValue)
        {
            var value = _config[key];
            int parsed;
            if (int.TryParse(value, out parsed))
                return parsed;

            return defaultValue;
        }

        private static bool TentaObterPropriedade(JsonElement elemento, string nome, out JsonElement propriedade)
        {
            propriedade = default;
            if (elemento.ValueKind != JsonValueKind.Object)
                return false;
            return elemento.TryGetProperty(nome, out propriedade);
        }

        private static string ObterTexto(JsonElement elemento, string nome)
        {
            JsonElement propriedade;
            if (!TentaObterPropriedade(elemento, nome, out propriedade))
                return null;

            if (propriedade.ValueKind == JsonValueKind.String)
                return propriedade.GetString();

            return propriedade.ToString();
        }

        private static decimal? ObterDecimal(JsonElement elemento, string nome)
        {
            JsonElement propriedade;
            if (!TentaObterPropriedade(elemento, nome, out propriedade))
                return null;

            if (propriedade.ValueKind == JsonValueKind.Number)
                return propriedade.GetDecimal();

            if (propriedade.ValueKind == JsonValueKind.String)
            {
                decimal valor;
                if (decimal.TryParse(propriedade.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
                    return valor;
            }

            return null;
        }

        private static DateTime? ObterDataHora(JsonElement elemento)
        {
            var texto = ObterTexto(elemento, "recvTime")
                ?? ObterTexto(elemento, "recvtime")
                ?? ObterTexto(elemento, "timestamp")
                ?? ObterTexto(elemento, "date")
                ?? ObterTexto(elemento, "time");

            if (string.IsNullOrWhiteSpace(texto))
                return null;

            DateTime data;
            if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out data))
                return data;

            return null;
        }

        private class ContextoAquario
        {
            public int AquarioId { get; set; }
            public string NomeAquario { get; set; }
            public string EntityId { get; set; }
        }

        private class PontoHistorico
        {
            public string Atributo { get; set; }
            public decimal? Valor { get; set; }
            public string Texto { get; set; }
            public DateTime DataLeituraUtc { get; set; }
        }
    }
}