using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using PBL.DAO;
using PBL.Models;
using PBL.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PBL.Controllers
{
    public class SmartLampController : Controller
    {
        private readonly IConfiguration _config;
        private readonly SmartLampMqttService _mqtt;
        private readonly SmartLampConfigDAO _dao = new SmartLampConfigDAO();

        public SmartLampController(IConfiguration config, SmartLampMqttService mqtt)
        {
            _config = config;
            _mqtt = mqtt;
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

        private int ResolverAquarioId()
        {
            if (int.TryParse(_config["FiwareSthComet:DefaultAquarioId"], out var idConfig) && idConfig > 0)
                return idConfig;

            var aquarios = new AquarioDAO().Listagem();
            if (aquarios != null && aquarios.Any())
                return aquarios.First().Id;

            return 0;
        }

        private void PreencherViewBagsComuns()
        {
            var aquarioId = ResolverAquarioId();
            ViewBag.Aquarios = new SelectList(new AquarioDAO().Listagem(), "Id", "Nome", aquarioId);
            ViewBag.Modos = new SelectList(new[]
            {
                new { Value = 0, Text = "0 - Desligada" },
                new { Value = 1, Text = "1 - Fraca" },
                new { Value = 2, Text = "2 - Média" },
                new { Value = 3, Text = "3 - Forte" },
                new { Value = 4, Text = "4 - Personalizada" }
            }, "Value", "Text");
            ViewBag.BrokerUrl = _config["SmartLampMqtt:BrokerUrl"];
            ViewBag.TopicSensor = _config["SmartLampMqtt:TopicSensor"];
        }

        [HttpGet]
        public IActionResult Personalizar()
        {
            try
            {
                PreencherViewBagsComuns();
                var aquarioId = ResolverAquarioId();
                if (aquarioId <= 0)
                {
                    ViewBag.Mensagem = "Cadastre pelo menos um aquário antes de configurar a lâmpada.";
                    return View(new SmartLampConfigViewModel());
                }

                var model = _dao.ConsultaOuCriaPadrao(aquarioId);
                ViewBag.Mensagem = TempData["Mensagem"];
                return View(model);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Salvar(SmartLampConfigViewModel model)
        {
            try
            {
                if (model.AquarioId <= 0)
                    model.AquarioId = ResolverAquarioId();

                if (model.AquarioId <= 0)
                {
                    PreencherViewBagsComuns();
                    ViewBag.Modos = new SelectList(new[]
                    {
                        new { Value = 0, Text = "0 - Desligada" },
                        new { Value = 1, Text = "1 - Fraca" },
                        new { Value = 2, Text = "2 - Média" },
                        new { Value = 3, Text = "3 - Forte" },
                        new { Value = 4, Text = "4 - Personalizada" }
                    }, "Value", "Text", model.Modo);
                    ViewBag.Mensagem = "Cadastre pelo menos um aquário antes de salvar a configuração da lâmpada.";
                    return View("Personalizar", model);
                }

                model.Brilho = Math.Max(0, Math.Min(100, model.Brilho));
                model.R = Math.Max(0, Math.Min(255, model.R));
                model.G = Math.Max(0, Math.Min(255, model.G));
                model.B = Math.Max(0, Math.Min(255, model.B));
                if (model.LuzAlvo.HasValue)
                    model.LuzAlvo = Math.Max(0, Math.Min(100, model.LuzAlvo.Value));

                if (!ModelState.IsValid)
                {
                    PreencherViewBagsComuns();
                    ViewBag.Modos = new SelectList(new[]
                    {
                        new { Value = 0, Text = "0 - Desligada" },
                        new { Value = 1, Text = "1 - Fraca" },
                        new { Value = 2, Text = "2 - Média" },
                        new { Value = 3, Text = "3 - Forte" },
                        new { Value = 4, Text = "4 - Personalizada" }
                    }, "Value", "Text", model.Modo);
                    return View("Personalizar", model);
                }

                _dao.Salvar(model);

                // Publica JSON no formato esperado pelo ESP32 (campo "tipo":"peixe")
                await _mqtt.PublicarLuzAsync(model.R, model.G, model.B, model.Brilho);

                TempData["Mensagem"] = "Configuração salva e enviada para a lâmpada (MQTT).";
                return RedirectToAction("Personalizar");
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return RedirectToAction("Personalizar");
        }
    }
}
