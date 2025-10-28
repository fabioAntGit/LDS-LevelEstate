using API.Models;
using API.RankingsBadges.Rankings.DTOs;
using API.RankingsBadges.Rankings.Exceptions;
using API.RankingsBadges.Rankings.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.RankingsBadges.Rankings.Controllers
{
    [Route("api/rankings")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class RankingController : ControllerBase
    {
        public readonly IRankingService rankingService;

        public RankingController(IRankingService rankingService)
        {
            this.rankingService = rankingService;
        }

        [HttpGet("getRanking")]
        [ProducesResponseType(typeof(RankingDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Ranking>> GetRankingOf([FromQuery] int? month, [FromQuery] int year)
        {
            try
            {
                var ranking = await rankingService.GetRankingOf(month, year);

                return Ok(ranking);
            }
            catch (RankingNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }

        }

        [HttpGet("update")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> Update([FromQuery] int points)
        {
            try
            {
                var idAgent = 2;// AUTH NOT IMPLEMENTED YET

                var ranking = await rankingService.Update(idAgent, points);

                return Ok(ranking);
            }
            catch (RankingNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
