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
            {
                context.Result = RedirectToAction("Index", "Login");
                return;
            }

            ViewBag.Logado = true;
            base.OnActionExecuting(context);
        }

        public IActionResult Index()
        {
            try
            {
                var aquarios = new AquarioDAO().Listagem();

                ViewBag.Aquarios = new SelectList(aquarios, "Id", "Nome");
                ViewBag.OrigemDados = _historicoService.EstaConfigurado
                    ? "STH-Comet com MongoDB"
                    : "Banco local (SQL Server)";

                return View();
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        [HttpGet]
        public async Task<IActionResult> DadosFiltrados(
            int? aquarioId,
            DateTime? dataInicio,
            DateTime? dataFim,
            decimal? temperaturaMin,
            decimal? temperaturaMax,
            decimal? tempArMin,
            decimal? tempArMax,
            decimal? tdsMin,
            decimal? tdsMax,
            decimal? salinidadeMin,
            decimal? salinidadeMax,
            decimal? nivelMin,
            decimal? nivelMax,
            string qualidade)
        {
            try
            {
                var leituras = await _historicoService.ConsultarHistoricoAsync(
                    aquarioId,
                    null,
                    null,
                    lastN: 100
                );

                leituras = AplicarFiltros(
                    leituras,
                    aquarioId,
                    dataInicio,
                    dataFim,
                    temperaturaMin,
                    temperaturaMax,
                    tempArMin,
                    tempArMax,
                    tdsMin,
                    tdsMax,
                    salinidadeMin,
                    salinidadeMax,
                    nivelMin,
                    nivelMax,
                    qualidade);

                if (!leituras.Any() && !_historicoService.EstaConfigurado)
                {
                    leituras = new LeituraSensorDAO()
                        .ConsultaDashboard(aquarioId, dataInicio, dataFim);
                }

                return PartialView("_TabelaLeituras", leituras);
            }
            catch (Exception erro)
            {
                return Content(
                    $"<div class='alert alert-danger'>Erro: {erro.Message}</div>",
                    "text/html"
                );
            }
        }

        [HttpGet]
        public async Task<IActionResult> DadosGrafico(
            int? aquarioId,
            DateTime? dataInicio,
            DateTime? dataFim,
            decimal? temperaturaMin,
            decimal? temperaturaMax,
            decimal? tempArMin,
            decimal? tempArMax,
            decimal? tdsMin,
            decimal? tdsMax,
            decimal? salinidadeMin,
            decimal? salinidadeMax,
            decimal? nivelMin,
            decimal? nivelMax,
            string qualidade)
        {
            try
            {
                var leituras = await _historicoService.ConsultarHistoricoAsync(
                    aquarioId,
                    null,
                    null,
                    lastN: 100
                );

                leituras = AplicarFiltros(
                    leituras,
                    aquarioId,
                    dataInicio,
                    dataFim,
                    temperaturaMin,
                    temperaturaMax,
                    tempArMin,
                    tempArMax,
                    tdsMin,
                    tdsMax,
                    salinidadeMin,
                    salinidadeMax,
                    nivelMin,
                    nivelMax,
                    qualidade);

                var dados = leituras
                    .OrderBy(x => x.DataLeitura)
                    .Select(x => new
                    {
                        dataHora = x.DataLeitura.ToString("dd/MM HH:mm"),
                        tempAgua = x.TemperaturaAgua,
                        tempAr = x.TemperaturaAr,
                        umidade = x.UmidadeAr,
                        tdsPpm = x.TdsPpm,
                        ec = x.TdsPpm,
                        salinidadePpt = x.SalinidadePpt,
                        qualidadeAgua = !string.IsNullOrWhiteSpace(x.QualidadeAgua) ? x.QualidadeAgua : x.QualidadeTds,
                        nivel = x.NivelPct,
                        volume = x.VolumeLitros,
                        ldr = x.LdrRaw
                    });

                return Json(dados);
            }
            catch (Exception erro)
            {
                return BadRequest(new { erro = erro.Message });
            }
        }

        private static System.Collections.Generic.List<LeituraSensorViewModel> AplicarFiltros(
            System.Collections.Generic.List<LeituraSensorViewModel> leituras,
            int? aquarioId,
            DateTime? dataInicio,
            DateTime? dataFim,
            decimal? temperaturaMin,
            decimal? temperaturaMax,
            decimal? tempArMin,
            decimal? tempArMax,
            decimal? tdsMin,
            decimal? tdsMax,
            decimal? salinidadeMin,
            decimal? salinidadeMax,
            decimal? nivelMin,
            decimal? nivelMax,
            string qualidade)
        {
            if (aquarioId.HasValue && aquarioId.Value > 0)
            {
                leituras = leituras
                    .Where(x => x.AquarioId == aquarioId.Value)
                    .ToList();
            }

            if (dataInicio.HasValue)
            {
                leituras = leituras
                    .Where(x => x.DataLeitura.Date >= dataInicio.Value.Date)
                    .ToList();
            }

            if (dataFim.HasValue)
            {
                leituras = leituras
                    .Where(x => x.DataLeitura.Date <= dataFim.Value.Date)
                    .ToList();
            }

            if (temperaturaMin.HasValue)
            {
                leituras = leituras
                    .Where(x => x.TemperaturaAgua.HasValue && x.TemperaturaAgua.Value >= temperaturaMin.Value)
                    .ToList();
            }

            if (temperaturaMax.HasValue)
            {
                leituras = leituras
                    .Where(x => x.TemperaturaAgua.HasValue && x.TemperaturaAgua.Value <= temperaturaMax.Value)
                    .ToList();
            }

            if (tempArMin.HasValue)
            {
                leituras = leituras
                    .Where(x => x.TemperaturaAr.HasValue && x.TemperaturaAr.Value >= tempArMin.Value)
                    .ToList();
            }

            if (tempArMax.HasValue)
            {
                leituras = leituras
                    .Where(x => x.TemperaturaAr.HasValue && x.TemperaturaAr.Value <= tempArMax.Value)
                    .ToList();
            }

            if (tdsMin.HasValue)
            {
                leituras = leituras
                    .Where(x => x.TdsPpm.HasValue && x.TdsPpm.Value >= tdsMin.Value)
                    .ToList();
            }

            if (tdsMax.HasValue)
            {
                leituras = leituras
                    .Where(x => x.TdsPpm.HasValue && x.TdsPpm.Value <= tdsMax.Value)
                    .ToList();
            }

            if (salinidadeMin.HasValue)
            {
                leituras = leituras
                    .Where(x => x.SalinidadePpt.HasValue && x.SalinidadePpt.Value >= salinidadeMin.Value)
                    .ToList();
            }

            if (salinidadeMax.HasValue)
            {
                leituras = leituras
                    .Where(x => x.SalinidadePpt.HasValue && x.SalinidadePpt.Value <= salinidadeMax.Value)
                    .ToList();
            }

            if (nivelMin.HasValue)
            {
                leituras = leituras
                    .Where(x => x.NivelPct.HasValue && x.NivelPct.Value >= nivelMin.Value)
                    .ToList();
            }

            if (nivelMax.HasValue)
            {
                leituras = leituras
                    .Where(x => x.NivelPct.HasValue && x.NivelPct.Value <= nivelMax.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(qualidade))
            {
                var q = qualidade.Trim();
                leituras = leituras
                    .Where(x => (!string.IsNullOrWhiteSpace(x.QualidadeAgua) && x.QualidadeAgua.Contains(q, StringComparison.OrdinalIgnoreCase))
                                || (!string.IsNullOrWhiteSpace(x.QualidadeTds) && x.QualidadeTds.Contains(q, StringComparison.OrdinalIgnoreCase))
                                || (!string.IsNullOrWhiteSpace(x.Alerta) && x.Alerta.Contains(q, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return leituras
                .OrderByDescending(x => x.DataLeitura)
                .ToList();
        }
    }
}