using PBL.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System;

namespace PBL.DAO
{
    public class UsuarioDAO : PadraoDAO<UsuarioViewModel>
    {
        public void Inserir(UsuarioViewModel usuario)
        {
            HelperDAO.ExecutaProc("spIncluiUsuario", CriaParametros(usuario));
        }

        protected override SqlParameter[] CriaParametros(UsuarioViewModel usuario)
        {
            SqlParameter[] parametros = new SqlParameter[4];
            parametros[0] = new SqlParameter("id", usuario.Id);
            parametros[1] = new SqlParameter("nome", usuario.Nome);
            parametros[2] = new SqlParameter("login", usuario.Login);
            parametros[3] = new SqlParameter("senha", usuario.Senha);
            return parametros;
        }

        protected override UsuarioViewModel MontaModel(DataRow registro)
        {
            return MontaUsuario(registro);
        }

        public UsuarioViewModel ConsultaPorLogin(string login)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("login", login)
            };
            DataTable tabela = HelperDAO.ExecutaProcSelect("spConsultaUsuarioPorLogin", p);
            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaUsuario(tabela.Rows[0]);
        }

        public override UsuarioViewModel Consulta(int id)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("id", id)
            };
            DataTable tabela = HelperDAO.ExecutaProcSelect("spConsultaUsuario", p);
            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaUsuario(tabela.Rows[0]);
        }

        public override List<UsuarioViewModel> ConsultaTodos()
        {
            DataTable tabela = HelperDAO.ExecutaProcSelect(NomeSpListagem, new SqlParameter[] { new SqlParameter("tabela", "Usuarios") });
            List<UsuarioViewModel> lista = new List<UsuarioViewModel>();
            foreach (DataRow linha in tabela.Rows)
            {
                lista.Add(MontaUsuario(linha));
            }
            return lista;
        }

        private UsuarioViewModel MontaUsuario(DataRow registro)
        {
            UsuarioViewModel u = new UsuarioViewModel();
            u.Id = Convert.ToInt32(registro["id"]);
            u.Nome = registro["nome"].ToString();
            u.Login = registro["login"].ToString();
            u.Senha = registro["senha"].ToString();
            return u;
        }

        protected override void SetTabela()
        {
            Tabela = "Usuarios";
        }
    }
}
