using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL.DAO;
using PBL.Models;
using PBL.Services;
using Microsoft.Extensions.Logging;
using System.Net;
using System;
using System.IO;
using System.Threading.Tasks;
namespace PBL.Controllers
{
    public class PeixeController : PadraoController<PeixeViewModel>
    {
        private readonly IWebHostEnvironment _env;
        private readonly FishAiService _fishAi;
        private readonly SmartLampMqttService _mqtt;
        private readonly SmartLampConfigDAO _lampDao = new SmartLampConfigDAO();
        private readonly ILogger<PeixeController> _logger;

        public PeixeController(IWebHostEnvironment env, FishAiService fishAi, SmartLampMqttService mqtt, ILogger<PeixeController> logger)
        {
            _env = env;
            _fishAi = fishAi;
            _mqtt = mqtt;
            _logger = logger;
            DAO = new PeixeDAO();
            GeraProximoId = true;
        }

        protected override void PreencheDadosParaView(string operacao, PeixeViewModel model)
        {
            base.PreencheDadosParaView(operacao, model);
            var aquarios = new AquarioDAO().Listagem();
            ViewBag.Aquarios = new SelectList(aquarios, "Id", "Nome", model.AquarioId);
        }

        protected override void ValidaDados(PeixeViewModel model, string operacao)
        {
            base.ValidaDados(model, operacao);
            // Limpar espaços e codificar HTML pra evitar XSS simples
            // Não é proteção completa mas ajuda contra ataques óbvios no form
            model.Nome = WebUtility.HtmlEncode(model.Nome?.Trim());
            model.Especie = WebUtility.HtmlEncode(model.Especie?.Trim());
            model.NomeCientifico = WebUtility.HtmlEncode(model.NomeCientifico?.Trim());

            if (string.IsNullOrWhiteSpace(model.Nome))
                ModelState.AddModelError("Nome", "Informe o nome do peixe.");
            if (string.IsNullOrWhiteSpace(model.Especie))
                ModelState.AddModelError("Especie", "Informe a espécie.");
            if (model.TamanhoCm <= 0)
                ModelState.AddModelError("TamanhoCm", "Tamanho deve ser maior que zero.");
            if (model.AquarioId <= 0)
                ModelState.AddModelError("AquarioId", "Selecione um aquário.");

            // Validar coerência dos parâmetros ambientais
            if (!model.Parameters.IsValid(out string parametersError))
                ModelState.AddModelError("Parameters", parametersError);
        }

        public override IActionResult Save(PeixeViewModel model, string Operacao)
        {
            try
            {
                _logger?.LogInformation("Salvando peixe Id={Id} Operacao={Operacao}", model?.Id, Operacao);
                
                // Processa arquivo de foto se enviado
                // Senão, em edição, mantém a foto antiga
                var arquivoFoto = Request.Form.Files["arquivoFoto"];
                if (arquivoFoto != null && arquivoFoto.Length > 0)
                    model.Foto = SalvarFoto(arquivoFoto, model.Id);
                else if (Operacao == "A")
                {
                    var existente = DAO.Consulta(model.Id);
                    if (existente != null)
                        model.Foto = existente.Foto;
                }

                // Roda validações antes de salvar
                ValidaDados(model, Operacao);
                if (ModelState.IsValid == false)
                {
                    ViewBag.Operacao = Operacao;
                    PreencheDadosParaView(Operacao, model);
                    return View(NomeViewForm, model);
                }

                // Registra quando o peixe foi adicionado/modificado
                // Útil para auditoria e para identificar alterações posteriores dos parâmetros
                model.Parameters.UpdatedAt = System.DateTime.Now;

                if (Operacao == "I")
                    DAO.Insert(model);
                else
                    DAO.Update(model);

                _logger?.LogInformation("Peixe salvo com sucesso Id={Id}", model.Id);

                // Quando user cadastra um peixe, automaticamente aplica os parâmetros ideais à lâmpada
                // Se o peixe requer 40% de brilho, a lâmpada já vai pra 40%
                // Uso fire-and-forget (_) porque é não-crítico falhar aqui
                if (model.LuminosidadeIdeal.HasValue || model.TemperaturaIdeal.HasValue)
                {
                    _lampDao.AplicarAlvos(model.AquarioId, model.LuminosidadeIdeal, model.TemperaturaIdeal);
                    if (model.LuminosidadeIdeal.HasValue)
                    {
                        // Tenta enviar pra lâmpada MQTT se o chip esiver conectado
                        // Se falhar, DB fica atualizado mesmo assim
                        _ = _mqtt.AplicarBrilhoAsync(model.LuminosidadeIdeal.Value);
                    }
                }

                return RedirectToAction(NomeViewIndex);
            }
            catch (Exception erro)
            {
                _logger?.LogError(erro, "Erro ao salvar peixe");
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DetectarParametros(IFormFile arquivoFoto)
        {
            try
            {
                if (arquivoFoto == null || arquivoFoto.Length == 0)
                    return Json(new { sucesso = false, mensagem = "Envie uma imagem válida." });

                var pastaTemp = Path.Combine(_env.WebRootPath, "uploads", "temp");
                if (!Directory.Exists(pastaTemp))
                    Directory.CreateDirectory(pastaTemp);

                // Extrai extensão do arquivo. Se não tiver, assume JPG (é mais comum pra foto)
                var extensao = Path.GetExtension(arquivoFoto.FileName);
                if (string.IsNullOrWhiteSpace(extensao))
                    extensao = ".jpg";

                // Prefixo "peixe_ai" facilita identificar arquivos temporários deste fluxo
                var nomeArquivo = $"peixe_ai_{DateTime.Now:yyyyMMddHHmmssfff}{extensao}";
                var caminho = Path.Combine(pastaTemp, nomeArquivo);

                using (var stream = new FileStream(caminho, FileMode.Create))
                    await arquivoFoto.CopyToAsync(stream);

                var result = await _fishAi.AnalisarImagemAsync(caminho);
                // Deleta arquivo temporário - não precisa manter a imagem no servidor após a análise
                try { System.IO.File.Delete(caminho); } catch { }

                // Monta resposta com flag de origem para a UI indicar preenchimento automático
                var response = new
                {
                    sucesso = true,
                    especie = result.Especie,
                    nomeCientifico = result.NomeCientifico,
                    temperaturaIdeal = result.TemperaturaIdeal,
                    temperaturaMin = result.TemperaturaMin,
                    temperaturaMax = result.TemperaturaMax,
                    luminosidadeIdeal = result.LuminosidadeIdeal,
                    luminosidadeMin = result.LuminosidadeMin,
                    luminosidadeMax = result.LuminosidadeMax,
                    tdsPpmMin = result.TdsPpmMin,
                    tdsPpmMax = result.TdsPpmMax,
                    salinidadePptMin = result.SalinidadePptMin,
                    salinidadePptMax = result.SalinidadePptMax,
                    volumeMinLitros = result.VolumeMinLitros,
                    originFromAI = true,
                    updatedAt = System.DateTime.Now
                };

                return Json(response);
            }
            catch (Exception erro)
            {
                _logger?.LogError(erro, "Erro DetectarParametros");
                return Json(new { sucesso = false, mensagem = erro.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DetectarParametrosPorEspecie(string especie)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(especie))
                    return Json(new { sucesso = false, mensagem = "Informe a espécie." });
                especie = WebUtility.HtmlEncode(especie.Trim());
                var result = await _fishAi.AnalisarEspecieAsync(especie);
                return Json(new
                {
                    sucesso = true,
                    especie = result.Especie,
                    nomeCientifico = result.NomeCientifico,
                    temperaturaIdeal = result.TemperaturaIdeal,
                    temperaturaMin = result.TemperaturaMin,
                    temperaturaMax = result.TemperaturaMax,
                    luminosidadeIdeal = result.LuminosidadeIdeal,
                    luminosidadeMin = result.LuminosidadeMin,
                    luminosidadeMax = result.LuminosidadeMax,
                    tdsPpmMin = result.TdsPpmMin,
                    tdsPpmMax = result.TdsPpmMax,
                    salinidadePptMin = result.SalinidadePptMin,
                    salinidadePptMax = result.SalinidadePptMax,
                    volumeMinLitros = result.VolumeMinLitros,
                    originFromAI = true,
                    updatedAt = System.DateTime.Now
                });
            }
            catch (Exception erro)
            {
                _logger?.LogError(erro, "Erro DetectarParametrosPorEspecie");
                return Json(new { sucesso = false, mensagem = erro.Message });
            }
        }

        private string SalvarFoto(IFormFile arquivo, int id)
        {
            var pasta = Path.Combine(_env.WebRootPath, "uploads", "peixes");
            if (!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);

            var extensao = Path.GetExtension(arquivo.FileName);
            var nomeArquivo = $"peixe_{id}_{DateTime.Now:yyyyMMddHHmmss}{extensao}";
            var caminho = Path.Combine(pasta, nomeArquivo);

            using (var stream = new FileStream(caminho, FileMode.Create))
                arquivo.CopyTo(stream);

            return $"/uploads/peixes/{nomeArquivo}";
        }

        [HttpGet]
        public IActionResult InfoAquario(int id)
        {
            try
            {
                var aquario = new AquarioDAO().Consulta(id);
                if (aquario == null)
                    return Json(new { sucesso = false, mensagem = "Aquário não encontrado." });

                return Json(new
                {
                    sucesso = true,
                    nome = aquario.Nome,
                    capacidade = aquario.CapacidadeLitros,
                    tipoAgua = aquario.TipoAgua,
                    responsavel = aquario.NomeUsuario
                });
            }
            catch (Exception erro)
            {
                return Json(new { sucesso = false, mensagem = erro.Message });
            }
        }
    }
}
