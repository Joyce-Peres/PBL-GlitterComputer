using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL.DAO;
using PBL.Models;
using PBL.Services;
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

        public PeixeController(IWebHostEnvironment env, FishAiService fishAi, SmartLampMqttService mqtt)
        {
            _env = env;
            _fishAi = fishAi;
            _mqtt = mqtt;
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
            if (string.IsNullOrWhiteSpace(model.Nome))
                ModelState.AddModelError("Nome", "Informe o nome do peixe.");
            if (string.IsNullOrWhiteSpace(model.Especie))
                ModelState.AddModelError("Especie", "Informe a espécie.");
            if (model.TamanhoCm <= 0)
                ModelState.AddModelError("TamanhoCm", "Tamanho deve ser maior que zero.");
            if (model.AquarioId <= 0)
                ModelState.AddModelError("AquarioId", "Selecione um aquário.");
        }

        public override IActionResult Save(PeixeViewModel model, string Operacao)
        {
            try
            {
                var arquivoFoto = Request.Form.Files["arquivoFoto"];
                if (arquivoFoto != null && arquivoFoto.Length > 0)
                    model.Foto = SalvarFoto(arquivoFoto, model.Id);
                else if (Operacao == "A")
                {
                    var existente = DAO.Consulta(model.Id);
                    if (existente != null)
                        model.Foto = existente.Foto;
                }

                // Repete a lógica do PadraoController para permitir ação pós-save.
                ValidaDados(model, Operacao);
                if (ModelState.IsValid == false)
                {
                    ViewBag.Operacao = Operacao;
                    PreencheDadosParaView(Operacao, model);
                    return View(NomeViewForm, model);
                }

                if (Operacao == "I")
                    DAO.Insert(model);
                else
                    DAO.Update(model);

                // Automação: aplica parâmetros ideais (se existirem) à configuração da lâmpada.
                if (model.LuminosidadeIdeal.HasValue || model.TemperaturaIdeal.HasValue)
                {
                    _lampDao.AplicarAlvos(model.AquarioId, model.LuminosidadeIdeal, model.TemperaturaIdeal);
                    if (model.LuminosidadeIdeal.HasValue)
                    {
                        // tenta enviar para a smart lamp automaticamente
                        _ = _mqtt.AplicarBrilhoAsync(model.LuminosidadeIdeal.Value);
                    }
                }

                return RedirectToAction(NomeViewIndex);
            }
            catch (Exception erro)
            {
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

                var extensao = Path.GetExtension(arquivoFoto.FileName);
                if (string.IsNullOrWhiteSpace(extensao))
                    extensao = ".jpg";

                var nomeArquivo = $"peixe_ai_{DateTime.Now:yyyyMMddHHmmssfff}{extensao}";
                var caminho = Path.Combine(pastaTemp, nomeArquivo);

                using (var stream = new FileStream(caminho, FileMode.Create))
                    await arquivoFoto.CopyToAsync(stream);

                var result = await _fishAi.AnalisarImagemAsync(caminho);
                try { System.IO.File.Delete(caminho); } catch { }

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
                    phMin = result.PhMin,
                    phMax = result.PhMax
                });
            }
            catch (Exception erro)
            {
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
                    phMin = result.PhMin,
                    phMax = result.PhMax
                });
            }
            catch (Exception erro)
            {
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
