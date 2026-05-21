using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL.DAO;
using PBL.Models;
using PBL.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PBL.Controllers
{
    public class DashboardController : Controller
    {
        private readonly FiwareSthCometService _historicoService;

        public DashboardController(FiwareSthCometService historicoService)
        {
            _historicoService = historicoService;
        }

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
                ViewBag.OrigemDados = _historicoService.EstaConfigurado
                    ? "MongoDB via STH-Comet"
                    : "SQL legado";
                return View();
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        [HttpGet]
        public async Task<IActionResult> DadosFiltrados(int? aquarioId, DateTime? dataInicio, DateTime? dataFim)
        {
            try
            {
                var leituras = await _historicoService.ConsultarHistoricoAsync(aquarioId, dataInicio, dataFim);
                if (!leituras.Any() && !_historicoService.EstaConfigurado)
                    leituras = new LeituraSensorDAO().ConsultaDashboard(aquarioId, dataInicio, dataFim);
                return PartialView("_TabelaLeituras", leituras);
            }
            catch (Exception erro)
            {
                return Content($"<div class='alert alert-danger'>Erro: {erro.Message}</div>");
            }
        }
    }
}
