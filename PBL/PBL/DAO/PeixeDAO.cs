using PBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PBL.DAO
{
    public class PeixeDAO : PadraoDAO<PeixeViewModel>
    {
        protected override void SetTabela()
        {
            Tabela = "Peixes";
        }

        protected override SqlParameter[] CriaParametros(PeixeViewModel model)
        {
            return new SqlParameter[]
            {
                new SqlParameter("id", model.Id),
                new SqlParameter("nome", model.Nome ?? ""),
                new SqlParameter("especie", model.Especie ?? ""),
                new SqlParameter("tamanhoCm", model.TamanhoCm),
                new SqlParameter("aquarioId", model.AquarioId),
                new SqlParameter("foto", (object)model.Foto ?? DBNull.Value)
            };
        }

        protected override PeixeViewModel MontaModel(DataRow registro)
        {
            var model = new PeixeViewModel
            {
                Id = Convert.ToInt32(registro["id"]),
                Nome = registro["nome"].ToString(),
                Especie = registro["especie"].ToString(),
                TamanhoCm = Convert.ToDecimal(registro["tamanhoCm"]),
                AquarioId = Convert.ToInt32(registro["aquarioId"]),
                Foto = registro["foto"] != DBNull.Value ? registro["foto"].ToString() : null
            };
            if (registro.Table.Columns.Contains("nomeAquario") && registro["nomeAquario"] != DBNull.Value)
                model.NomeAquario = registro["nomeAquario"].ToString();
            return model;
        }

        public List<PeixeViewModel> ConsultaComFiltro(string nome, string especie, int? aquarioId)
        {
            var parametros = new SqlParameter[]
            {
                new SqlParameter("nome", (object)nome ?? DBNull.Value),
                new SqlParameter("especie", (object)especie ?? DBNull.Value),
                new SqlParameter("aquarioId", aquarioId.HasValue && aquarioId.Value > 0 ? aquarioId.Value : (object)DBNull.Value)
            };
            var tabela = HelperDAO.ExecutaProcSelect("spConsultaPeixesFiltro", parametros);
            var lista = new List<PeixeViewModel>();
            foreach (DataRow linha in tabela.Rows)
                lista.Add(MontaModel(linha));
            return lista;
        }
    }
}
