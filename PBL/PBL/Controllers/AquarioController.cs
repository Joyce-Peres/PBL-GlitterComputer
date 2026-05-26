using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL.DAO;
using PBL.Models;
using System;
using System.Linq;

namespace PBL.Controllers
{
    public class AquarioController : PadraoController<AquarioViewModel>
    {
        public AquarioController()
        {
            DAO = new AquarioDAO();
            GeraProximoId = true;
        }

        protected override void PreencheDadosParaView(string operacao, AquarioViewModel model)
        {
            base.PreencheDadosParaView(operacao, model);
            DefinirUsuarioLogado(model, operacao);
            ViewBag.TiposAgua = new SelectList(new[] { "Doce", "Salgada", "Mista" });
        }

        public override IActionResult Save(AquarioViewModel model, string Operacao)
        {
            DefinirUsuarioLogado(model, Operacao);
            return base.Save(model, Operacao);
        }

        private void DefinirUsuarioLogado(AquarioViewModel model, string operacao)
        {
            var usuarioId = ObterUsuarioIdDaSessao();
            if (usuarioId > 0)
                model.UsuarioId = usuarioId;
            else if (operacao == "A" && model.UsuarioId <= 0)
            {
                var existente = DAO.Consulta(model.Id);
                if (existente != null)
                    model.UsuarioId = existente.UsuarioId;
            }
        }

        private int ObterUsuarioIdDaSessao()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId.HasValue && usuarioId.Value > 0)
                return usuarioId.Value;

            var login = HttpContext.Session.GetString("UsuarioLogin");
            if (string.IsNullOrWhiteSpace(login))
                return 0;

            var usuario = new UsuarioDAO().ConsultaPorLogin(login);
            if (usuario == null)
                return 0;

            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            return usuario.Id;
        }

        protected override void ValidaDados(AquarioViewModel model, string operacao)
        {
            base.ValidaDados(model, operacao);
            if (model.UsuarioId <= 0)
                ModelState.AddModelError("", "Usuário não identificado na sessão. Faça logoff e entre novamente.");
            if (string.IsNullOrWhiteSpace(model.Nome))
                ModelState.AddModelError("Nome", "Informe o nome do aquário.");
            if (model.CapacidadeLitros <= 0)
                ModelState.AddModelError("CapacidadeLitros", "Capacidade deve ser maior que zero.");
            if (string.IsNullOrWhiteSpace(model.TipoAgua))
                ModelState.AddModelError("TipoAgua", "Selecione o tipo de água.");
        }

        public override IActionResult Delete(int id)
        {
            try
            {
                var aquario = DAO.Consulta(id);
                if (aquario == null)
                    return RedirectToAction(NomeViewIndex);

                var peixeDao = new PeixeDAO();
                var peixes = peixeDao.ConsultaComFiltro(null, null, id);
                var lampConfig = new SmartLampConfigDAO().ConsultaPorAquario(id);

                ViewBag.QtdPeixes = peixes?.Count ?? 0;
                ViewBag.TemLampConfig = lampConfig != null;
                ViewBag.MensagemConfirmacao = ViewBag.QtdPeixes > 0
                    ? $"Este aquário possui {ViewBag.QtdPeixes} peixe(s) vinculado(s). Ao confirmar, o sistema vai remover o aquário, os peixes, as leituras e a configuração da Smart Lamp associados."
                    : (ViewBag.TemLampConfig == true
                        ? "Este aquário possui configuração de Smart Lamp associada. Ao confirmar, o sistema vai remover o aquário e os dados relacionados."
                        : "Confirme a exclusão do aquário.");

                return View("DeleteConfirm", aquario);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        [HttpPost]
        public IActionResult ExcluirConfirmado(int id)
        {
            try
            {
                ((AquarioDAO)DAO).ExcluirComDependentes(id);
                TempData["Mensagem"] = "Aquário excluído com sucesso.";
                return RedirectToAction(NomeViewIndex);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }
        }
    }
