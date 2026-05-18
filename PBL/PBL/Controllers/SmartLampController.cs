using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using PBL.DAO;
using PBL.Models;
using PBL.Services;
using System;
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

        [HttpGet]
        public IActionResult Personalizar(int? aquarioId)
        {
            try
            {
                var aquarios = new AquarioDAO().Listagem();
                ViewBag.Aquarios = new SelectList(aquarios, "Id", "Nome", aquarioId);
                ViewBag.Modos = new SelectList(new[]
                {
                    new { Value = 0, Text = "0 - Desligada" },
                    new { Value = 1, Text = "1 - Fraca" },
                    new { Value = 2, Text = "2 - Média" },
                    new { Value = 3, Text = "3 - Forte" },
                    new { Value = 4, Text = "4 - Personalizada" }
                }, "Value", "Text");

                if (!aquarioId.HasValue)
                {
                    if (aquarios.Count > 0)
                        aquarioId = aquarios[0].Id;
                }

                if (!aquarioId.HasValue)
                    return View(new SmartLampConfigViewModel());

                var model = _dao.ConsultaOuCriaPadrao(aquarioId.Value);
                ViewBag.BrokerUrl = _config["SmartLampMqtt:BrokerUrl"];
                ViewBag.TopicSensor = _config["SmartLampMqtt:TopicSensor"];

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
                    ModelState.AddModelError("AquarioId", "Selecione um aquário.");
                model.Brilho = Math.Max(0, Math.Min(100, model.Brilho));
                model.R = Math.Max(0, Math.Min(255, model.R));
                model.G = Math.Max(0, Math.Min(255, model.G));
                model.B = Math.Max(0, Math.Min(255, model.B));
                if (model.LuzAlvo.HasValue)
                    model.LuzAlvo = Math.Max(0, Math.Min(100, model.LuzAlvo.Value));

                if (!ModelState.IsValid)
                {
                    var aquarios = new AquarioDAO().Listagem();
                    ViewBag.Aquarios = new SelectList(aquarios, "Id", "Nome", model.AquarioId);
                    ViewBag.Modos = new SelectList(new[]
                    {
                        new { Value = 0, Text = "0 - Desligada" },
                        new { Value = 1, Text = "1 - Fraca" },
                        new { Value = 2, Text = "2 - Média" },
                        new { Value = 3, Text = "3 - Forte" },
                        new { Value = 4, Text = "4 - Personalizada" }
                    }, "Value", "Text", model.Modo);
                    ViewBag.BrokerUrl = _config["SmartLampMqtt:BrokerUrl"];
                    ViewBag.TopicSensor = _config["SmartLampMqtt:TopicSensor"];
                    return View("Personalizar", model);
                }

                _dao.Salvar(model);

                // Envia comandos básicos para a lâmpada (se MQTT estiver configurado).
                await _mqtt.PublicarAsync(
                    $"setMode|{model.Modo}",
                    $"setRGB|{model.R},{model.G},{model.B}",
                    $"setBrightness|{model.Brilho}");

                TempData["Mensagem"] = "Configuração salva e enviada para a lâmpada (MQTT).";
                return RedirectToAction("Personalizar", new { aquarioId = model.AquarioId });
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewBag.BrokerUrl = _config["SmartLampMqtt:BrokerUrl"];
            ViewBag.TopicSensor = _config["SmartLampMqtt:TopicSensor"];
            return View();
        }
    }
}
