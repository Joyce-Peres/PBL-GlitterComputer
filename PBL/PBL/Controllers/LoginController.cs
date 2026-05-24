using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PBL.DAO;
using PBL.Models;

namespace PBL.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Logado = HelperControllers.VerificaUserLogado(HttpContext.Session);
            return View();
        }

        public IActionResult FazLogin(string usuario, string senha)
        {
            UsuarioDAO usuarioDAO = new UsuarioDAO();
            UsuarioViewModel usuarioLogado = usuarioDAO.ConsultaPorLogin(usuario);

            if (usuarioLogado != null && usuarioLogado.Senha == senha)
            {
                HttpContext.Session.SetString("Logado", "true");
                HttpContext.Session.SetInt32("UsuarioId", usuarioLogado.Id);
                HttpContext.Session.SetString("UsuarioNome", usuarioLogado.Nome);
                HttpContext.Session.SetString("UsuarioLogin", usuarioLogado.Login);
                return RedirectToAction("index", "Home");
            }
            else
            {
                ViewBag.Erro = "Usuário ou senha inválidos!";
                return View("Index");
            }
        }

        public IActionResult LogOff()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Cadastrar()
        {
            return RedirectToAction("Index", new { tab = "signup" });
        }

        [HttpGet]
        public IActionResult VerificarLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return Json(new { disponivel = false, mensagem = "Informe um e-mail." });

            var usuarioDAO = new UsuarioDAO();
            var existente = usuarioDAO.ConsultaPorLogin(login);
            return Json(new
            {
                disponivel = existente == null,
                mensagem = existente == null ? "E-mail disponível." : "Este e-mail já está cadastrado."
            });
        }

        public IActionResult FazCadastro(string nome, string login, string senha, string confirmarSenha)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.ActivePane = "signup";
                ViewBag.CadastroErro = "Todos os campos são obrigatórios!";
                return View("Index");
            }

            if (senha != confirmarSenha)
            {
                ViewBag.ActivePane = "signup";
                ViewBag.CadastroErro = "As senhas não conferem!";
                return View("Index");
            }

            UsuarioDAO usuarioDAO = new UsuarioDAO();
            UsuarioViewModel usuarioExistente = usuarioDAO.ConsultaPorLogin(login);

            if (usuarioExistente != null)
            {
                ViewBag.ActivePane = "signup";
                ViewBag.CadastroErro = "Este e-mail já está cadastrado!";
                return View("Index");
            }

            UsuarioViewModel novoUsuario = new UsuarioViewModel
            {
                Nome = nome,
                Login = login,
                Senha = senha
            };

            try
            {
                usuarioDAO.Inserir(novoUsuario);
                ViewBag.Sucesso = "Cadastro realizado com sucesso! Faça login para continuar.";
                ViewBag.ActivePane = "login";
                return View("Index");
            }
            catch
            {
                ViewBag.ActivePane = "signup";
                ViewBag.CadastroErro = "Erro ao realizar cadastro. Tente novamente.";
                return View("Index");
            }
        }
    }
}
