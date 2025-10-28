using API.Models.Enums;
using API.Perfis.Agente.Exceptions;
using API.Perfis.Agente.Interfaces;
using API.PesquisasFiltros.Pesquisas.DTOs;
using API.RankingsBadges.Badges.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Perfis.Agente.Controllers
{
    [Route("api/agent")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class AgentController : ControllerBase
    {
        private IAgentService agentService;

        public AgentController(IAgentService agentService)
        {
            this.agentService = agentService;
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(PropertyListDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PropertyListDTO> GetAgents([FromQuery] int? position, [FromQuery] TypeRanking? typeRanking, [FromQuery] string? name)
        {
            try
            {
                var agents = agentService.GetAgents(position, typeRanking, name);

                return Ok(agents);
            }
            catch (AgentNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (BadgePositionNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (Exception e)
            {
                return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
