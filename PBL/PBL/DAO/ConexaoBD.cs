using System;
using System.Data.SqlClient;

namespace PBL.DAO
{
    public static class ConexaoBD
    {
        public static SqlConnection GetConexao()
        {
            string strCon = Environment.GetEnvironmentVariable("PBL_CONNECTION_STRING");
            bool usaVariavelAmbiente = !string.IsNullOrWhiteSpace(strCon);
            if (!usaVariavelAmbiente)
            {
                strCon = "Data Source=LOCALHOST;Initial Catalog=PBL;Integrated Security=True;TrustServerCertificate=True";
            }
            SqlConnection conexao = new SqlConnection(strCon);
            try
            {
                conexao.Open();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Falha ao abrir conexão com o banco usando {(usaVariavelAmbiente ? "PBL_CONNECTION_STRING" : "configuração padrão local")}.", ex);
            }
            return conexao;
        }
    }
}
