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
                return View();
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

                ViewBag.TotalPeixes = lista?.Count ?? 0;
                ViewBag.TotalEspecies = lista == null ? 0 : lista
                    .Where(x => !string.IsNullOrWhiteSpace(x.Especie))
                    .Select(x => x.Especie.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
                ViewBag.ComFoto = lista == null ? 0 : lista.Count(x => !string.IsNullOrWhiteSpace(x.Foto));
                ViewBag.ComParametrosCompletos = lista == null ? 0 : lista.Count(x =>
                    x.TemperaturaIdeal.HasValue &&
                    x.LuminosidadeIdeal.HasValue &&
                    x.TdsPpmMin.HasValue &&
                    x.TdsPpmMax.HasValue &&
                    x.SalinidadePptMin.HasValue &&
                    x.SalinidadePptMax.HasValue &&
                    x.VolumeMinLitros.HasValue);

                return PartialView("_TabelaPeixes", lista);
            }
            catch (Exception erro)
            {
                return Content($"<div class='alert alert-danger'>Erro: {erro.Message}</div>");
            }
        }

    }
}
