using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PBL.DAO;
using PBL.Models;
using PBL.Models.Api;
using PBL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBL.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class LeiturasController : ControllerBase
    {
        private readonly FiwareSthCometService _historicoService;

        public LeiturasController(FiwareSthCometService historicoService)
        {
            _historicoService = historicoService;
        }


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LeituraSensorViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LeituraSensorViewModel>>> Get(
            [FromQuery] int? aquarioId,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim)
        {
            try
            {
                var lista = await _historicoService.ConsultarHistoricoAsync(aquarioId, dataInicio, dataFim, lastN: 20);
                if (!lista.Any() && !_historicoService.EstaConfigurado)
                    lista = new LeituraSensorDAO().Listagem();
                return Ok(lista);
            }
            catch (Exception erro)
            {
                return StatusCode(500, new ApiErrorResponse { Erro = erro.Message });
            }
        }



        [HttpGet("aquario/{aquarioId}")]
        [ProducesResponseType(typeof(IEnumerable<LeituraSensorViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LeituraSensorViewModel>>> GetPorAquario(
            int aquarioId,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim)
        {
            try
            {
                var lista = await _historicoService.ConsultarHistoricoAsync(aquarioId, dataInicio, dataFim, lastN: 20);
                if (!lista.Any() && !_historicoService.EstaConfigurado)
                    lista = new LeituraSensorDAO().ConsultaComFiltro(aquarioId, null, null, null, null);
                return Ok(lista);
            }
            catch (Exception erro)
            {
                return StatusCode(500, new ApiErrorResponse { Erro = erro.Message });
            }
        }


        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(LeituraRegistroResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] LeituraIoTRequest request)
        {
            try
            {
                if (request == null || request.AquarioId <= 0)
                    return BadRequest(new ApiErrorResponse
                    {
                        Erro = "Dados inválidos. Informe aquarioId, temperatura e nivelAgua."
                    });

                if (!ModelState.IsValid)
                {
                    var primeiroErro = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault() ?? "Dados inválidos.";
                    return BadRequest(new ApiErrorResponse { Erro = primeiroErro });
                }

                new LeituraSensorDAO().InserirLeituraIoT(
                    request.AquarioId, request.Temperatura, request.NivelAgua,
                    request.TdsPpm, request.SalinidadePpt, request.QualidadeTds);

                return Ok(new LeituraRegistroResponse
                {
                    Mensagem = "Leitura registrada com sucesso.",
                    AquarioId = request.AquarioId,
                    DataLeitura = DateTime.Now
                });
            }
            catch (Exception erro)
            {
                return StatusCode(500, new ApiErrorResponse { Erro = erro.Message });
            }
        }
    }
}
