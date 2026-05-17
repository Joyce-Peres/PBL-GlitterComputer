using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL.DAO;
using PBL.Models;
using System;

namespace PBL.Controllers
{
    public class ConsultaController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!HelperControllers.VerificaUserLogado(HttpContext.Session))
                context.Result = RedirectToAction("Index", "Login");
            else
            {
                ViewBag.Logado = true;
                base.OnActionExecuting(context);
            }
        }

        public IActionResult Peixes(string nome, string especie, int? aquarioId)
        {
            try
            {
                var aquarios = new AquarioDAO().Listagem();
                ViewBag.Aquarios = new SelectList(aquarios, "Id", "Nome", aquarioId);
                ViewBag.NomeFiltro = nome;
                ViewBag.EspecieFiltro = especie;
                var lista = new PeixeDAO().ConsultaComFiltro(nome, especie, aquarioId);
                return View(lista);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        [HttpGet]
        public IActionResult PeixesAjax(string nome, string especie, int? aquarioId)
        {
            try
            {
                var lista = new PeixeDAO().ConsultaComFiltro(nome, especie, aquarioId);
                return PartialView("_TabelaPeixes", lista);
            }
            catch (Exception erro)
            {
                return Content($"<div class='alert alert-danger'>Erro: {erro.Message}</div>");
            }
        }

        public IActionResult Leituras(int? aquarioId, DateTime? dataInicio, DateTime? dataFim,
            decimal? temperaturaMin, decimal? temperaturaMax)
        {
            try
            {
                var aquarios = new AquarioDAO().Listagem();
                ViewBag.Aquarios = new SelectList(aquarios, "Id", "Nome", aquarioId);
                ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
                ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");
                ViewBag.TemperaturaMin = temperaturaMin;
                ViewBag.TemperaturaMax = temperaturaMax;
                var lista = new LeituraSensorDAO().ConsultaComFiltro(aquarioId, dataInicio, dataFim, temperaturaMin, temperaturaMax);
                return View(lista);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }
    }
}
