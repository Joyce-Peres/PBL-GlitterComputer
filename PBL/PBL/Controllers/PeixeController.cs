using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL.DAO;
using PBL.Models;
using System;
using System.IO;
namespace PBL.Controllers
{
    public class PeixeController : PadraoController<PeixeViewModel>
    {
        private readonly IWebHostEnvironment _env;

        public PeixeController(IWebHostEnvironment env)
        {
            _env = env;
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

                return base.Save(model, Operacao);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
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
