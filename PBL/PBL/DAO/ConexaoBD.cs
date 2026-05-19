using System;
using System.Data.SqlClient;

namespace PBL.DAO
{
    public static class ConexaoBD
    {
        public static SqlConnection GetConexao()
        {
            string strCon = Environment.GetEnvironmentVariable("PBL_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(strCon))
            {
                strCon = "Data Source=LOCALHOST;Initial Catalog=PBL;user id=sa; password=123456";
            }
            SqlConnection conexao = new SqlConnection(strCon);
            conexao.Open();
            return conexao;
        }
    }
}
