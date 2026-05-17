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
                new SqlParameter("usuarioId", model.UsuarioId)
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
            return model;
        }

        public List<AquarioViewModel> ConsultaPorUsuario(int usuarioId)
        {
            var lista = Listagem();
            return lista.FindAll(a => a.UsuarioId == usuarioId);
        }
    }
}
