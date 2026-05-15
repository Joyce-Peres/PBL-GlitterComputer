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
            return View();
        }

        public IActionResult FazCadastro(string nome, string login, string senha, string confirmarSenha)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Erro = "Todos os campos são obrigatórios!";
                return View("Cadastrar");
            }

            if (senha != confirmarSenha)
            {
                ViewBag.Erro = "As senhas não conferem!";
                return View("Cadastrar");
            }

            UsuarioDAO usuarioDAO = new UsuarioDAO();
            UsuarioViewModel usuarioExistente = usuarioDAO.ConsultaPorLogin(login);

            if (usuarioExistente != null)
            {
                ViewBag.Erro = "Este e-mail já está cadastrado!";
                return View("Cadastrar");
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
                return View("Index");
            }
            catch
            {
                ViewBag.Erro = "Erro ao realizar cadastro. Tente novamente.";
                return View("Cadastrar");
            }
        }
    }
}
