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
                var leituras = await _historicoService.ConsultarHistoricoAsync(
                    aquarioId,
                    dataInicio,
                    dataFim,
                    lastN: 20
                );

                if (!leituras.Any() && !_historicoService.EstaConfigurado)
                    leituras = new LeituraSensorDAO().ConsultaDashboard(aquarioId, dataInicio, dataFim);

                return PartialView("_TabelaLeituras", leituras);
            }
            catch (Exception erro)
            {
                return Content($"<div class='alert alert-danger'>Erro ao consultar o histórico FIWARE/STH-Comet: {erro.Message}</div>", "text/html");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DadosGrafico(int? aquarioId)
        {
            try
            {
                var leituras = await _historicoService.ConsultarHistoricoAsync(
                    aquarioId,
                    null,
                    null,
                    lastN: 20
                );

                if (!leituras.Any() && !_historicoService.EstaConfigurado)
                    leituras = new LeituraSensorDAO().ConsultaDashboard(aquarioId, null, null);

                var dados = leituras
                    .OrderBy(item => item.DataLeitura)
                    .Select(item => new
                    {
                        dataHora = item.DataLeitura.ToString("HH:mm:ss"),
                        dataCompleta = item.DataLeitura.ToString("dd/MM/yyyy HH:mm:ss"),
                        aquario = item.NomeAquario,
                        tempAgua = item.TemperaturaAgua,
                        tempAr = item.TemperaturaAr,
                        umidade = item.UmidadeAr,
                        ec = item.TdsPpm,
                        nivel = item.NivelPct,
                        volume = item.VolumeLitros,
                        ldr = item.LdrRaw,
                        qualidade = item.QualidadeAgua
                    })
                    .ToList();

                return Json(dados);
            }
            catch (Exception erro)
            {
                Response.StatusCode = 500;
                return Json(new { erro = erro.Message });
            }
        }
    }
}
