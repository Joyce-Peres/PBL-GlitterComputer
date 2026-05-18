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
                new SqlParameter("nomeCientifico", (object)model.NomeCientifico ?? DBNull.Value),
                new SqlParameter("temperaturaIdeal", (object)model.TemperaturaIdeal ?? DBNull.Value),
                new SqlParameter("luminosidadeIdeal", (object)model.LuminosidadeIdeal ?? DBNull.Value),
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
                NomeCientifico = registro.Table.Columns.Contains("nomeCientifico") && registro["nomeCientifico"] != DBNull.Value
                    ? registro["nomeCientifico"].ToString()
                    : null,
                TemperaturaIdeal = registro.Table.Columns.Contains("temperaturaIdeal") && registro["temperaturaIdeal"] != DBNull.Value
                    ? (decimal?)Convert.ToDecimal(registro["temperaturaIdeal"])
                    : null,
                LuminosidadeIdeal = registro.Table.Columns.Contains("luminosidadeIdeal") && registro["luminosidadeIdeal"] != DBNull.Value
                    ? (int?)Convert.ToInt32(registro["luminosidadeIdeal"])
                    : null,
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
