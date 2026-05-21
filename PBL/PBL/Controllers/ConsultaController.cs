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
    public class ConsultaController : Controller
    {
        private readonly FiwareSthCometService _historicoService;

        public ConsultaController(FiwareSthCometService historicoService)
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

        public async Task<IActionResult> Leituras(int? aquarioId, DateTime? dataInicio, DateTime? dataFim,
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
                ViewBag.OrigemDados = _historicoService.EstaConfigurado
                    ? "MongoDB via STH-Comet"
                    : "SQL legado";

                var lista = await _historicoService.ConsultarHistoricoAsync(aquarioId, dataInicio, dataFim);
                if (!lista.Any() && !_historicoService.EstaConfigurado)
                    lista = new LeituraSensorDAO().ConsultaComFiltro(aquarioId, dataInicio, dataFim, temperaturaMin, temperaturaMax);

                if (temperaturaMin.HasValue || temperaturaMax.HasValue)
                {
                    lista = lista.FindAll(item =>
                    {
                        var temperatura = item.TemperaturaAgua.HasValue ? item.TemperaturaAgua.Value : item.Temperatura;
                        if (temperaturaMin.HasValue && temperatura < temperaturaMin.Value)
                            return false;
                        if (temperaturaMax.HasValue && temperatura > temperaturaMax.Value)
                            return false;
                        return true;
                    });
                }

                return View(lista);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }
    }
}
