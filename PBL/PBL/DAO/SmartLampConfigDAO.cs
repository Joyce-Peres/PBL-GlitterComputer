using PBL.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PBL.DAO
{
    public class SmartLampConfigDAO
    {
        private bool AquarioExiste(int aquarioId)
        {
            if (aquarioId <= 0)
                return false;

            return new AquarioDAO().Listagem().Any(a => a.Id == aquarioId);
        }

        public SmartLampConfigViewModel ConsultaPorAquario(int aquarioId)
        {
            if (!AquarioExiste(aquarioId))
                return null;

            var parametros = new SqlParameter[] { new SqlParameter("aquarioId", aquarioId) };
            var table = HelperDAO.ExecutaProcSelect("spConsultaLampConfig", parametros);
            
            if (table.Rows.Count == 0)
                return null;

            return MontaModel(table.Rows[0]);
        }

        public SmartLampConfigViewModel ConsultaOuCriaPadrao(int aquarioId)
        {
            if (!AquarioExiste(aquarioId))
                return new SmartLampConfigViewModel { AquarioId = aquarioId };

            var atual = ConsultaPorAquario(aquarioId);
            if (atual != null)
                return atual;

            var padrao = new SmartLampConfigViewModel { AquarioId = aquarioId };
            Salvar(padrao);
            return ConsultaPorAquario(aquarioId);
        }

        public void Salvar(SmartLampConfigViewModel model)
        {
            if (!AquarioExiste(model.AquarioId))
                throw new InvalidOperationException("Não é possível salvar a configuração da lâmpada porque o aquário informado não existe.");

            var parametros = new SqlParameter[]
            {
                new SqlParameter("aquarioId", model.AquarioId),
                new SqlParameter("modo", model.Modo),
                new SqlParameter("brilho", model.Brilho),
                new SqlParameter("r", model.R),
                new SqlParameter("g", model.G),
                new SqlParameter("b", model.B),
                new SqlParameter("luzAlvo", (object)model.LuzAlvo ?? DBNull.Value),
                new SqlParameter("tempAlvo", (object)model.TempAlvo ?? DBNull.Value)
            };
            HelperDAO.ExecutaProc("spSalvarLampConfig", parametros);
        }

        public void AplicarAlvos(int aquarioId, int? luzAlvo, decimal? tempAlvo)
        {
            if (!AquarioExiste(aquarioId))
                return;

            var parametros = new SqlParameter[]
            {
                new SqlParameter("aquarioId", aquarioId),
                new SqlParameter("luzAlvo", (object)luzAlvo ?? DBNull.Value),
                new SqlParameter("tempAlvo", (object)tempAlvo ?? DBNull.Value)
            };
            HelperDAO.ExecutaProc("spAplicarAlvosLamp", parametros);
        }

        private SmartLampConfigViewModel MontaModel(DataRow row)
        {
            return new SmartLampConfigViewModel
            {
                AquarioId = Convert.ToInt32(row["aquarioId"]),
                NomeAquario = row["nomeAquario"].ToString(),
                Modo = Convert.ToInt32(row["modo"]),
                Brilho = Convert.ToInt32(row["brilho"]),
                R = Convert.ToInt32(row["r"]),
                G = Convert.ToInt32(row["g"]),
                B = Convert.ToInt32(row["b"]),
                LuzAlvo = row["luzAlvo"] != DBNull.Value ? (int?)Convert.ToInt32(row["luzAlvo"]) : null,
                TempAlvo = row["tempAlvo"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["tempAlvo"]) : null,
                AtualizadoEm = row["atualizadoEm"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["atualizadoEm"]) : null
            };
        }
    }
}
