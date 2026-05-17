using Microsoft.AspNetCore.Mvc;
using PBL.DAO;
using PBL.Models;
using System;
using System.Collections.Generic;

namespace PBL.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeiturasController : ControllerBase
    {
        /// <summary>
        /// Retorna todas as leituras dos sensores IoT do aquário inteligente.
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<LeituraSensorViewModel>> Get()
        {
            try
            {
                var lista = new LeituraSensorDAO().Listagem();
                return Ok(lista);
            }
            catch (Exception erro)
            {
                return StatusCode(500, new { erro = erro.Message });
            }
        }

        /// <summary>
        /// Retorna leituras filtradas por aquário.
        /// </summary>
        [HttpGet("aquario/{aquarioId}")]
        public ActionResult<IEnumerable<LeituraSensorViewModel>> GetPorAquario(int aquarioId)
        {
            try
            {
                var lista = new LeituraSensorDAO().ConsultaComFiltro(aquarioId, null, null, null, null);
                return Ok(lista);
            }
            catch (Exception erro)
            {
                return StatusCode(500, new { erro = erro.Message });
            }
        }

        /// <summary>
        /// Registra uma nova leitura enviada pelo dispositivo IoT.
        /// </summary>
        [HttpPost]
        public IActionResult Post([FromBody] LeituraIoTRequest request)
        {
            try
            {
                if (request == null || request.AquarioId <= 0)
                    return BadRequest(new { erro = "Dados inválidos. Informe aquarioId, temperatura, ph e nivelAgua." });

                new LeituraSensorDAO().InserirLeituraIoT(
                    request.AquarioId, request.Temperatura, request.Ph, request.NivelAgua);

                return Ok(new
                {
                    mensagem = "Leitura registrada com sucesso.",
                    aquarioId = request.AquarioId,
                    dataLeitura = DateTime.Now
                });
            }
            catch (Exception erro)
            {
                return StatusCode(500, new { erro = erro.Message });
            }
        }
    }

    public class LeituraIoTRequest
    {
        public int AquarioId { get; set; }
        public decimal Temperatura { get; set; }
        public decimal Ph { get; set; }
        public decimal NivelAgua { get; set; }
    }
}
