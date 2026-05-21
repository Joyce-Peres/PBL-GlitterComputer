using Microsoft.AspNetCore.Mvc;
using PBL.DAO;
using PBL.Models;
using PBL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBL.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeiturasController : ControllerBase
    {
        private readonly FiwareSthCometService _historicoService;

        public LeiturasController(FiwareSthCometService historicoService)
        {
            _historicoService = historicoService;
        }

        /// <summary>
        /// Retorna o histórico das leituras IoT consultado no STH-Comet.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeituraSensorViewModel>>> Get([FromQuery] int? aquarioId, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            try
            {
                var lista = await _historicoService.ConsultarHistoricoAsync(aquarioId, dataInicio, dataFim);
                if (!lista.Any() && !_historicoService.EstaConfigurado)
                    lista = new LeituraSensorDAO().Listagem();
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
        public async Task<ActionResult<IEnumerable<LeituraSensorViewModel>>> GetPorAquario(int aquarioId, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            try
            {
                var lista = await _historicoService.ConsultarHistoricoAsync(aquarioId, dataInicio, dataFim);
                if (!lista.Any() && !_historicoService.EstaConfigurado)
                    lista = new LeituraSensorDAO().ConsultaComFiltro(aquarioId, null, null, null, null);
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
