using PBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PBL.DAO
{
    public class AquarioDAO : PadraoDAO<AquarioViewModel>
    {
        protected override void SetTabela()
        {
            Tabela = "Aquarios";
        }

        protected override SqlParameter[] CriaParametros(AquarioViewModel model)
        {
            return new SqlParameter[]
            {
                new SqlParameter("id", model.Id),
                new SqlParameter("nome", model.Nome ?? ""),
                new SqlParameter("capacidadeLitros", model.CapacidadeLitros),
                new SqlParameter("tipoAgua", model.TipoAgua ?? ""),
                new SqlParameter("usuarioId", model.UsuarioId),
                new SqlParameter("fiwareEntityId", string.IsNullOrWhiteSpace(model.FiwareEntityId) ? (object)DBNull.Value : model.FiwareEntityId.Trim())
            };
        }

        protected override AquarioViewModel MontaModel(DataRow registro)
        {
            var model = new AquarioViewModel
            {
                Id = Convert.ToInt32(registro["id"]),
                Nome = registro["nome"].ToString(),
                CapacidadeLitros = Convert.ToDecimal(registro["capacidadeLitros"]),
                TipoAgua = registro["tipoAgua"].ToString(),
                UsuarioId = Convert.ToInt32(registro["usuarioId"])
            };
            if (registro.Table.Columns.Contains("nomeUsuario") && registro["nomeUsuario"] != DBNull.Value)
                model.NomeUsuario = registro["nomeUsuario"].ToString();
            if (registro.Table.Columns.Contains("fiwareEntityId") && registro["fiwareEntityId"] != DBNull.Value)
                model.FiwareEntityId = registro["fiwareEntityId"].ToString();
            return model;
        }

        public AquarioViewModel ConsultaPorFiwareEntityId(string fiwareEntityId)
        {
            if (string.IsNullOrWhiteSpace(fiwareEntityId))
                return null;

            var normalizado = fiwareEntityId.Trim();
            var lista = Listagem();
            return lista.Find(a => string.Equals(a.FiwareEntityId, normalizado, StringComparison.OrdinalIgnoreCase));
        }

        public List<AquarioViewModel> ConsultaPorUsuario(int usuarioId)
        {
            var lista = Listagem();
            return lista.FindAll(a => a.UsuarioId == usuarioId);
        }

        public int QuantidadePeixes(int aquarioId)
        {
            var parametros = new SqlParameter[] { new SqlParameter("aquarioId", aquarioId) };
            var tabela = HelperDAO.ExecutaSelect("SELECT COUNT(1) AS total FROM Peixes WHERE aquarioId = @aquarioId", parametros);
            if (tabela.Rows.Count == 0)
                return 0;

            return Convert.ToInt32(tabela.Rows[0]["total"]);
        }

        public void ExcluirComDependentes(int aquarioId)
        {
            using (var conexao = ConexaoBD.GetConexao())
            using (var transacao = conexao.BeginTransaction())
            {
                try
                {
                    ExecutarComando(conexao, transacao, "DELETE FROM LampConfigs WHERE aquarioId = @aquarioId", aquarioId);
                    ExecutarComando(conexao, transacao, "DELETE FROM LeiturasSensor WHERE aquarioId = @aquarioId", aquarioId);
                    ExecutarComando(conexao, transacao, "DELETE FROM Peixes WHERE aquarioId = @aquarioId", aquarioId);
                    ExecutarComando(conexao, transacao, "DELETE FROM Aquarios WHERE id = @aquarioId", aquarioId);
                    transacao.Commit();
                }
                catch
                {
                    transacao.Rollback();
                    throw;
                }
            }
        }

        private static void ExecutarComando(SqlConnection conexao, SqlTransaction transacao, string sql, int aquarioId)
        {
            using (var comando = new SqlCommand(sql, conexao, transacao))
            {
                comando.Parameters.AddWithValue("@aquarioId", aquarioId);
                comando.ExecuteNonQuery();
            }
        }
    }
}
