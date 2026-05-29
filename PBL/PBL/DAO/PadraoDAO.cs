using PBL.DAO;
using PBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PBL.DAO
{
    public abstract class PadraoDAO<T> where T : PadraoViewModel
    {
        // DAO genérico que serve pra todas as entidades (Peixes, Aquarios, etc)
        // Cada subclasse só precisa implementar SetTabela() e CriaParametros()
        // O resto (Insert, Update, Delete, Consulta) funciona pra tudo via Stored Procedures
        public PadraoDAO()
        {
            SetTabela();
        } 
 
        protected string Tabela { get; set; }
        protected string NomeSpListagem { get; set; } = "spListagem";
        protected abstract SqlParameter[] CriaParametros(T model);
        protected abstract T MontaModel(DataRow registro);
        protected abstract void SetTabela();


        public virtual void Insert(T model)
        {
            // Chama spInsert_Peixes, spInsert_Aquarios, etc
            HelperDAO.ExecutaProc("spInsert_" + Tabela, CriaParametros(model));
        }

        public virtual void Update(T model)
        {
            HelperDAO.ExecutaProc("spUpdate_" + Tabela, CriaParametros(model));
        }

        public virtual void Delete(int id)
        {
            // Usa uma SP genérica que deleta de qualquer tabela
            // Mais seguro que SQL dinâmico ou concatenação
            var p = new SqlParameter[]
            {
                new SqlParameter("id", id),
                new SqlParameter("tabela", Tabela)
            };
            HelperDAO.ExecutaProc("spDelete", p);
        }

        public virtual T Consulta(int id)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("id", id),
                new SqlParameter("tabela", Tabela)
            };
            var tabela = HelperDAO.ExecutaProcSelect("spConsulta", p);
            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaModel(tabela.Rows[0]);
        }

        public virtual int ProximoId()
        {
            var p = new SqlParameter[]
              {
                new SqlParameter("tabela", Tabela)
              };
            var tabela = HelperDAO.ExecutaProcSelect("spProximoId", p);
            return Convert.ToInt32(tabela.Rows[0][0]);
        }

        public virtual List<T> Listagem()
        {
            // Listagem padrão ordenada pelo primeiro campo (ID)
            // Subclasses podem fazer override pra usar procedures customizadas
            // ex: ConsultaPeixesFiltro em PeixeDAO pra filtrar por aquário
            var p = new SqlParameter[]
             {
                new SqlParameter("tabela", Tabela),
                new SqlParameter("Ordem", "1") // 1 é o primeiro campo da tabela 
             };
            var tabela = HelperDAO.ExecutaProcSelect(NomeSpListagem, p);
            List<T> lista = new List<T>();
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaModel(registro));

            return lista;
        }

        public virtual List<T> ConsultaTodos()
        {
            return Listagem();
        }
    }
}