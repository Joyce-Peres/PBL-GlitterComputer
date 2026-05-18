using PBL.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace PBL.DAO
{
    public class SmartLampConfigDAO
    {
        public SmartLampConfigViewModel ConsultaPorAquario(int aquarioId)
        {
            var sql = @"
SELECT lc.aquarioId, aq.nome AS nomeAquario,
       lc.modo, lc.brilho, lc.r, lc.g, lc.b,
       lc.luzAlvo, lc.tempAlvo, lc.atualizadoEm
FROM LampConfigs lc
INNER JOIN Aquarios aq ON aq.id = lc.aquarioId
WHERE lc.aquarioId = @aquarioId";

            using var con = ConexaoBD.GetConexao();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@aquarioId", aquarioId);

            using var da = new SqlDataAdapter(cmd);
            var table = new DataTable();
            da.Fill(table);

            if (table.Rows.Count == 0)
                return null;

            return MontaModel(table.Rows[0]);
        }

        public SmartLampConfigViewModel ConsultaOuCriaPadrao(int aquarioId)
        {
            var atual = ConsultaPorAquario(aquarioId);
            if (atual != null)
                return atual;

            var padrao = new SmartLampConfigViewModel { AquarioId = aquarioId };
            Salvar(padrao);
            return ConsultaPorAquario(aquarioId);
        }

        public void Salvar(SmartLampConfigViewModel model)
        {
            using var con = ConexaoBD.GetConexao();

            var existeSql = "SELECT COUNT(1) FROM LampConfigs WHERE aquarioId=@aquarioId";
            using var existeCmd = new SqlCommand(existeSql, con);
            existeCmd.Parameters.AddWithValue("@aquarioId", model.AquarioId);
            var existe = Convert.ToInt32(existeCmd.ExecuteScalar()) > 0;

            var sql = existe
                ? @"UPDATE LampConfigs
   SET modo=@modo, brilho=@brilho, r=@r, g=@g, b=@b,
       luzAlvo=@luzAlvo, tempAlvo=@tempAlvo, atualizadoEm=GETDATE()
 WHERE aquarioId=@aquarioId"
                : @"INSERT INTO LampConfigs (aquarioId, modo, brilho, r, g, b, luzAlvo, tempAlvo)
   VALUES (@aquarioId, @modo, @brilho, @r, @g, @b, @luzAlvo, @tempAlvo)";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@aquarioId", model.AquarioId);
            cmd.Parameters.AddWithValue("@modo", model.Modo);
            cmd.Parameters.AddWithValue("@brilho", model.Brilho);
            cmd.Parameters.AddWithValue("@r", model.R);
            cmd.Parameters.AddWithValue("@g", model.G);
            cmd.Parameters.AddWithValue("@b", model.B);
            cmd.Parameters.AddWithValue("@luzAlvo", (object)model.LuzAlvo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tempAlvo", (object)model.TempAlvo ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void AplicarAlvos(int aquarioId, int? luzAlvo, decimal? tempAlvo)
        {
            using var con = ConexaoBD.GetConexao();

            var sql = @"IF EXISTS (SELECT 1 FROM LampConfigs WHERE aquarioId=@aquarioId)
    UPDATE LampConfigs SET luzAlvo=@luzAlvo, tempAlvo=@tempAlvo, atualizadoEm=GETDATE() WHERE aquarioId=@aquarioId
ELSE
    INSERT INTO LampConfigs (aquarioId, luzAlvo, tempAlvo) VALUES (@aquarioId, @luzAlvo, @tempAlvo)";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@aquarioId", aquarioId);
            cmd.Parameters.AddWithValue("@luzAlvo", (object)luzAlvo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tempAlvo", (object)tempAlvo ?? DBNull.Value);
            cmd.ExecuteNonQuery();
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
