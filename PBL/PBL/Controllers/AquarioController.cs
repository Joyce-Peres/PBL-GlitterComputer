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
                // Verifica se existem peixes vinculados
                var peixeDao = new PeixeDAO();
                var peixes = peixeDao.ConsultaComFiltro(null, null, id);
                if (peixes != null && peixes.Any())
                {
                    return View("Error", new ErrorViewModel($"Não é possível excluir o aquário porque existem {peixes.Count} peixe(s) cadastrado(s). Remova-os primeiro."));
                }

                // Verifica se existe configuração de lâmpada vinculada
                var lampConfig = new SmartLampConfigDAO().ConsultaPorAquario(id);
                if (lampConfig != null)
                {
                    return View("Error", new ErrorViewModel("Não é possível excluir o aquário porque existe uma configuração de Smart Lamp associada. Remova-a primeiro."));
                }

                return base.Delete(id);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }
        }
    }
