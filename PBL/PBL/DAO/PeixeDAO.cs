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
                new SqlParameter("temperaturaMin", (object)model.TemperaturaMin ?? DBNull.Value),
                new SqlParameter("temperaturaMax", (object)model.TemperaturaMax ?? DBNull.Value),
                new SqlParameter("luminosidadeMin", (object)model.LuminosidadeMin ?? DBNull.Value),
                new SqlParameter("luminosidadeMax", (object)model.LuminosidadeMax ?? DBNull.Value),
                new SqlParameter("ph_min", (object)model.PhMin ?? DBNull.Value),
                new SqlParameter("ph_max", (object)model.PhMax ?? DBNull.Value),
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
                TemperaturaMin = registro.Table.Columns.Contains("temperaturaMin") && registro["temperaturaMin"] != DBNull.Value
                    ? (decimal?)Convert.ToDecimal(registro["temperaturaMin"]) 
                    : null,
                TemperaturaMax = registro.Table.Columns.Contains("temperaturaMax") && registro["temperaturaMax"] != DBNull.Value
                    ? (decimal?)Convert.ToDecimal(registro["temperaturaMax"]) 
                    : null,
                LuminosidadeIdeal = registro.Table.Columns.Contains("luminosidadeIdeal") && registro["luminosidadeIdeal"] != DBNull.Value
                    ? (int?)Convert.ToInt32(registro["luminosidadeIdeal"])
                    : null,
                LuminosidadeMin = registro.Table.Columns.Contains("luminosidadeMin") && registro["luminosidadeMin"] != DBNull.Value
                    ? (int?)Convert.ToInt32(registro["luminosidadeMin"]) 
                    : null,
                LuminosidadeMax = registro.Table.Columns.Contains("luminosidadeMax") && registro["luminosidadeMax"] != DBNull.Value
                    ? (int?)Convert.ToInt32(registro["luminosidadeMax"]) 
                    : null,
                TamanhoCm = Convert.ToDecimal(registro["tamanhoCm"]),
                AquarioId = Convert.ToInt32(registro["aquarioId"]),
                Foto = registro["foto"] != DBNull.Value ? registro["foto"].ToString() : null
            };
            if (registro.Table.Columns.Contains("ph_min") && registro["ph_min"] != DBNull.Value)
                model.PhMin = Convert.ToDecimal(registro["ph_min"]);
            if (registro.Table.Columns.Contains("ph_max") && registro["ph_max"] != DBNull.Value)
                model.PhMax = Convert.ToDecimal(registro["ph_max"]);
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
