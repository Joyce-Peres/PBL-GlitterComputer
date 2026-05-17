using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL.DAO;
using PBL.Models;
using System;

namespace PBL.Controllers
{
    public class DashboardController : Controller
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

        public IActionResult Index()
        {
            try
            {
                var aquarios = new AquarioDAO().Listagem();
                ViewBag.Aquarios = new SelectList(aquarios, "Id", "Nome");
                return View();
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        [HttpGet]
        public IActionResult DadosFiltrados(int? aquarioId, DateTime? dataInicio, DateTime? dataFim)
        {
            try
            {
                var leituras = new LeituraSensorDAO().ConsultaDashboard(aquarioId, dataInicio, dataFim);
                return PartialView("_TabelaLeituras", leituras);
            }
            catch (Exception erro)
            {
                return Content($"<div class='alert alert-danger'>Erro: {erro.Message}</div>");
            }
        }
    }
}
