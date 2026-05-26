using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using PBL.DAO;
using PBL.Models;
using PBL.Services;
using System;
using System.Collections.Generic;
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

        public IActionResult Index(int? aquarioId, DateTime? dataInicio, DateTime? dataFim,
            decimal? temperaturaMin, decimal? temperaturaMax,
            decimal? tempArMin, decimal? tempArMax,
            decimal? tdsMin, decimal? tdsMax,
            decimal? salinidadeMin, decimal? salinidadeMax,
            decimal? nivelMin, decimal? nivelMax,
            string qualidade)
        {
            try
            {
                var aquarios = new AquarioDAO().Listagem();
                ViewBag.Aquarios = new SelectList(aquarios, "Id", "Nome", aquarioId);
                ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
                ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");
                ViewBag.TemperaturaMin = temperaturaMin;
                ViewBag.TemperaturaMax = temperaturaMax;
                ViewBag.TempArMin = tempArMin;
                ViewBag.TempArMax = tempArMax;
                ViewBag.TdsMin = tdsMin;
                ViewBag.TdsMax = tdsMax;
                ViewBag.SalinidadeMin = salinidadeMin;
                ViewBag.SalinidadeMax = salinidadeMax;
                ViewBag.NivelMin = nivelMin;
                ViewBag.NivelMax = nivelMax;
                ViewBag.Qualidade = qualidade;
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
        public async Task<IActionResult> DadosFiltrados(int? aquarioId, DateTime? dataInicio, DateTime? dataFim,
            decimal? temperaturaMin, decimal? temperaturaMax,
            decimal? tempArMin, decimal? tempArMax,
            decimal? tdsMin, decimal? tdsMax,
            decimal? salinidadeMin, decimal? salinidadeMax,
            decimal? nivelMin, decimal? nivelMax,
            string qualidade)
        {
            try
            {
                var leituras = await ObterLeiturasBaseAsync(aquarioId, dataInicio, dataFim, temperaturaMin, temperaturaMax);
                leituras = AplicarFiltrosComplementares(
                    leituras,
                    tempArMin,
                    tempArMax,
                    tdsMin,
                    tdsMax,
                    salinidadeMin,
                    salinidadeMax,
                    nivelMin,
                    nivelMax,
                    qualidade);

                return PartialView("_TabelaLeituras", leituras);
            }
            catch (Exception erro)
            {
                return Content($"<div class='alert alert-danger'>Erro ao consultar o histórico FIWARE/STH-Comet: {erro.Message}</div>", "text/html");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DadosGrafico(int? aquarioId, DateTime? dataInicio, DateTime? dataFim,
            decimal? temperaturaMin, decimal? temperaturaMax,
            decimal? tempArMin, decimal? tempArMax,
            decimal? tdsMin, decimal? tdsMax,
            decimal? salinidadeMin, decimal? salinidadeMax,
            decimal? nivelMin, decimal? nivelMax,
            string qualidade)
        {
            try
            {
                var leituras = await ObterLeiturasBaseAsync(aquarioId, dataInicio, dataFim, temperaturaMin, temperaturaMax);
                leituras = AplicarFiltrosComplementares(
                    leituras,
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

        private async Task<List<LeituraSensorViewModel>> ObterLeiturasBaseAsync(int? aquarioId, DateTime? dataInicio, DateTime? dataFim,
            decimal? temperaturaMin, decimal? temperaturaMax)
        {
            var leituras = await _historicoService.ConsultarHistoricoAsync(
                aquarioId,
                dataInicio,
                dataFim,
                lastN: 20);

            if (!leituras.Any() && !_historicoService.EstaConfigurado)
            {
                if (temperaturaMin.HasValue || temperaturaMax.HasValue)
                    leituras = new LeituraSensorDAO().ConsultaComFiltro(aquarioId, dataInicio, dataFim, temperaturaMin, temperaturaMax);
                else
                    leituras = new LeituraSensorDAO().ConsultaDashboard(aquarioId, dataInicio, dataFim);
            }

            if (temperaturaMin.HasValue || temperaturaMax.HasValue)
            {
                leituras = leituras.FindAll(item =>
                {
                    var temperatura = item.TemperaturaAgua.HasValue ? item.TemperaturaAgua.Value : item.Temperatura;

                    if (temperaturaMin.HasValue && temperatura < temperaturaMin.Value)
                        return false;

                    if (temperaturaMax.HasValue && temperatura > temperaturaMax.Value)
                        return false;

                    return true;
                });
            }

            return leituras;
        }

        private List<LeituraSensorViewModel> AplicarFiltrosComplementares(IEnumerable<LeituraSensorViewModel> leituras,
            decimal? tempArMin, decimal? tempArMax,
            decimal? tdsMin, decimal? tdsMax,
            decimal? salinidadeMin, decimal? salinidadeMax,
            decimal? nivelMin, decimal? nivelMax,
            string qualidade)
        {
            return leituras
                .Where(item =>
                {
                    if (tempArMin.HasValue)
                    {
                        if (!item.TemperaturaAr.HasValue || item.TemperaturaAr.Value < tempArMin.Value)
                            return false;
                    }

                    if (tempArMax.HasValue)
                    {
                        if (!item.TemperaturaAr.HasValue || item.TemperaturaAr.Value > tempArMax.Value)
                            return false;
                    }

                    if (tdsMin.HasValue)
                    {
                        if (!item.TdsPpm.HasValue || item.TdsPpm.Value < tdsMin.Value)
                            return false;
                    }

                    if (tdsMax.HasValue)
                    {
                        if (!item.TdsPpm.HasValue || item.TdsPpm.Value > tdsMax.Value)
                            return false;
                    }

                    if (salinidadeMin.HasValue)
                    {
                        if (!item.SalinidadePpt.HasValue || item.SalinidadePpt.Value < salinidadeMin.Value)
                            return false;
                    }

                    if (salinidadeMax.HasValue)
                    {
                        if (!item.SalinidadePpt.HasValue || item.SalinidadePpt.Value > salinidadeMax.Value)
                            return false;
                    }

                    var nivel = item.NivelPct.HasValue ? item.NivelPct.Value : item.NivelAgua;

                    if (nivelMin.HasValue && nivel < nivelMin.Value)
                        return false;

                    if (nivelMax.HasValue && nivel > nivelMax.Value)
                        return false;

                    if (!string.IsNullOrWhiteSpace(qualidade))
                    {
                        var textoQualidade = item.QualidadeAgua ?? item.QualidadeTds ?? string.Empty;
                        if (textoQualidade.IndexOf(qualidade, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;
                    }

                    return true;
                })
                .ToList();
        }
    }
}
