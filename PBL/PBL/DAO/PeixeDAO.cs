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
                new SqlParameter("nome", (object)(model.Nome?.Trim() ?? string.Empty)),
                new SqlParameter("especie", (object)(model.Especie?.Trim() ?? string.Empty)),
                new SqlParameter("nomeCientifico", (object)(model.NomeCientifico?.Trim()) ?? DBNull.Value),
                new SqlParameter("temperaturaIdeal", (object)model.TemperaturaIdeal ?? DBNull.Value),
                new SqlParameter("luminosidadeIdeal", (object)model.LuminosidadeIdeal ?? DBNull.Value),
                new SqlParameter("temperaturaMin", (object)model.TemperaturaMin ?? DBNull.Value),
                new SqlParameter("temperaturaMax", (object)model.TemperaturaMax ?? DBNull.Value),
                new SqlParameter("luminosidadeMin", (object)model.LuminosidadeMin ?? DBNull.Value),
                new SqlParameter("luminosidadeMax", (object)model.LuminosidadeMax ?? DBNull.Value),
                new SqlParameter("tdsPpmMin", (object)model.TdsPpmMin ?? DBNull.Value),
                new SqlParameter("tdsPpmMax", (object)model.TdsPpmMax ?? DBNull.Value),
                new SqlParameter("salinidadePptMin", (object)model.SalinidadePptMin ?? DBNull.Value),
                new SqlParameter("salinidadePptMax", (object)model.SalinidadePptMax ?? DBNull.Value),
                new SqlParameter("volumeMinLitros", (object)model.VolumeMinLitros ?? DBNull.Value),
                new SqlParameter("originFromAI", (object)model.OriginFromAI ?? DBNull.Value),
                new SqlParameter("parametersUpdatedAt", (object)model.Parameters?.UpdatedAt ?? DBNull.Value),
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
                TdsPpmMin = registro.Table.Columns.Contains("tdsPpmMin") && registro["tdsPpmMin"] != DBNull.Value
                    ? (decimal?)Convert.ToDecimal(registro["tdsPpmMin"])
                    : null,
                TdsPpmMax = registro.Table.Columns.Contains("tdsPpmMax") && registro["tdsPpmMax"] != DBNull.Value
                    ? (decimal?)Convert.ToDecimal(registro["tdsPpmMax"])
                    : null,
                SalinidadePptMin = registro.Table.Columns.Contains("salinidadePptMin") && registro["salinidadePptMin"] != DBNull.Value
                    ? (decimal?)Convert.ToDecimal(registro["salinidadePptMin"])
                    : null,
                SalinidadePptMax = registro.Table.Columns.Contains("salinidadePptMax") && registro["salinidadePptMax"] != DBNull.Value
                    ? (decimal?)Convert.ToDecimal(registro["salinidadePptMax"])
                    : null,
                VolumeMinLitros = registro.Table.Columns.Contains("volumeMinLitros") && registro["volumeMinLitros"] != DBNull.Value
                    ? (decimal?)Convert.ToDecimal(registro["volumeMinLitros"])
                    : null,
                TamanhoCm = Convert.ToDecimal(registro["tamanhoCm"]),
                AquarioId = Convert.ToInt32(registro["aquarioId"]),
                Foto = registro["foto"] != DBNull.Value ? registro["foto"].ToString() : null
            };
            if (registro.Table.Columns.Contains("originFromAI") && registro["originFromAI"] != DBNull.Value)
                model.OriginFromAI = Convert.ToBoolean(registro["originFromAI"]);
            if (registro.Table.Columns.Contains("parametersUpdatedAt") && registro["parametersUpdatedAt"] != DBNull.Value)
                model.Parameters.UpdatedAt = Convert.ToDateTime(registro["parametersUpdatedAt"]);
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
