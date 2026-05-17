using Microsoft.AspNetCore.Mvc;

namespace PBL.Controllers
{
    public class SobreController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Logado = HelperControllers.VerificaUserLogado(HttpContext.Session);
            return View();
        }
    }
}
