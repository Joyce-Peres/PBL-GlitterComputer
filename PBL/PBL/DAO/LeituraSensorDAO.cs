using PBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PBL.DAO
{
    public class LeituraSensorDAO : PadraoDAO<LeituraSensorViewModel>
    {
        protected override void SetTabela()
        {
            Tabela = "LeiturasSensor";
        }

        protected override SqlParameter[] CriaParametros(LeituraSensorViewModel model)
        {
            return new SqlParameter[]
            {
                new SqlParameter("id", model.Id),
                new SqlParameter("aquarioId", model.AquarioId),
                new SqlParameter("temperatura", model.Temperatura),
                new SqlParameter("ph", model.Ph),
                new SqlParameter("nivelAgua", model.NivelAgua),
                new SqlParameter("dataLeitura", model.DataLeitura)
            };
        }

        protected override LeituraSensorViewModel MontaModel(DataRow registro)
        {
            var model = new LeituraSensorViewModel
            {
                Id = Convert.ToInt32(registro["id"]),
                AquarioId = Convert.ToInt32(registro["aquarioId"]),
                Temperatura = Convert.ToDecimal(registro["temperatura"]),
                Ph = Convert.ToDecimal(registro["ph"]),
                NivelAgua = Convert.ToDecimal(registro["nivelAgua"]),
                DataLeitura = Convert.ToDateTime(registro["dataLeitura"])
            };
            if (registro.Table.Columns.Contains("nomeAquario") && registro["nomeAquario"] != DBNull.Value)
                model.NomeAquario = registro["nomeAquario"].ToString();
            return model;
        }

        public List<LeituraSensorViewModel> ConsultaComFiltro(int? aquarioId, DateTime? dataInicio, DateTime? dataFim,
            decimal? temperaturaMin, decimal? temperaturaMax)
        {
            var parametros = new SqlParameter[]
            {
                new SqlParameter("aquarioId", aquarioId.HasValue && aquarioId.Value > 0 ? aquarioId.Value : (object)DBNull.Value),
                new SqlParameter("dataInicio", (object)dataInicio ?? DBNull.Value),
                new SqlParameter("dataFim", (object)dataFim ?? DBNull.Value),
                new SqlParameter("temperaturaMin", (object)temperaturaMin ?? DBNull.Value),
                new SqlParameter("temperaturaMax", (object)temperaturaMax ?? DBNull.Value)
            };
            var tabela = HelperDAO.ExecutaProcSelect("spConsultaLeiturasFiltro", parametros);
            var lista = new List<LeituraSensorViewModel>();
            foreach (DataRow linha in tabela.Rows)
                lista.Add(MontaModel(linha));
            return lista;
        }

        public List<LeituraSensorViewModel> ConsultaDashboard(int? aquarioId, DateTime? dataInicio, DateTime? dataFim)
        {
            var parametros = new SqlParameter[]
            {
                new SqlParameter("aquarioId", aquarioId.HasValue && aquarioId.Value > 0 ? aquarioId.Value : (object)DBNull.Value),
                new SqlParameter("dataInicio", (object)dataInicio ?? DBNull.Value),
                new SqlParameter("dataFim", (object)dataFim ?? DBNull.Value)
            };
            var tabela = HelperDAO.ExecutaProcSelect("spDashboardLeituras", parametros);
            var lista = new List<LeituraSensorViewModel>();
            foreach (DataRow linha in tabela.Rows)
                lista.Add(MontaModel(linha));
            return lista;
        }

        public void InserirLeituraIoT(int aquarioId, decimal temperatura, decimal ph, decimal nivelAgua)
        {
            var parametros = new SqlParameter[]
            {
                new SqlParameter("aquarioId", aquarioId),
                new SqlParameter("temperatura", temperatura),
                new SqlParameter("ph", ph),
                new SqlParameter("nivelAgua", nivelAgua)
            };
            HelperDAO.ExecutaProc("spInserirLeituraSensor", parametros);
        }
    }
}
