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
    /// <summary>
    /// Endpoints REST para consulta e registro de leituras dos sensores do aquário inteligente.
    /// </summary>
    /// <remarks>
    /// **Fluxo típico**
    /// 1. O dispositivo IoT envia leituras via `POST /api/leituras`.
    /// 2. O dashboard web e integrações consultam o histórico via `GET`.
    ///
    /// **Fonte dos dados (GET)**
    /// - Se o STH-Comet (FIWARE) estiver configurado em `appsettings.json`, o histórico vem do serviço externo.
    /// - Caso contrário, retorna dados do banco SQL Server local.
    ///
    /// **Autenticação:** esta API IoT é pública (sem token). Proteja o ambiente em produção conforme necessário.
    /// </remarks>
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

        /// <summary>
        /// Lista leituras de sensores com filtros opcionais.
        /// </summary>
        /// <param name="aquarioId">Filtra por aquário. Omita para trazer todos.</param>
        /// <param name="dataInicio">Data/hora inicial do período (opcional).</param>
        /// <param name="dataFim">Data/hora final do período (opcional).</param>
        /// <response code="200">Lista de leituras retornada com sucesso (pode ser vazia).</response>
        /// <response code="500">Erro ao consultar histórico.</response>
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

        /// <summary>
        /// Lista leituras de um aquário específico.
        /// </summary>
        /// <param name="aquarioId">Código do aquário.</param>
        /// <param name="dataInicio">Data/hora inicial do período (opcional).</param>
        /// <param name="dataFim">Data/hora final do período (opcional).</param>
        /// <response code="200">Leituras do aquário.</response>
        /// <response code="500">Erro ao consultar histórico.</response>
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

        /// <summary>
        /// Registra uma nova leitura enviada pelo dispositivo IoT.
        /// </summary>
        /// <remarks>
        /// Exemplo de corpo JSON:
        ///
        ///     {
        ///       "aquarioId": 1,
        ///       "temperatura": 25.5,
        ///       "nivelAgua": 85
        ///     }
        ///
        /// Campos gravados no banco: temperatura, nível de água e data/hora do servidor.
        /// </remarks>
        /// <param name="request">Dados dos sensores.</param>
        /// <response code="200">Leitura registrada.</response>
        /// <response code="400">Payload inválido ou aquarioId ausente.</response>
        /// <response code="500">Erro ao persistir no banco.</response>
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
