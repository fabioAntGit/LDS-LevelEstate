using API.ChatVisitasFeedback.Feedbacks.DTOs;
using API.ChatVisitasFeedback.Feedbacks.Exceptions;
using API.ChatVisitasFeedback.Feedbacks.Interfaces;
using API.ChatVisitasFeedback.Visitas.Exceptions;
using API.Perfis.Agente.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.ChatVisitasFeedback.Feedbacks.Controllers
{
    [Route("api/feedback")]
	[ApiController]
	[ProducesErrorResponseType(typeof(ProblemDetails))]
	public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            this.feedbackService = feedbackService;
        }


        [HttpPost]
		[ProducesResponseType(typeof(FeedbackInChatDTO), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public ActionResult<FeedbackInChatDTO> Submit (CreateFeedbackDTO dto)
        {
            try
            {
                // AUTH NOT IMPLEMENTED YET
                int userId = 2;

                var feedback = feedbackService.Submit(userId, dto.VisitId, dto.Rating, dto.Message);

                var feedbackDto = new FeedbackInChatDTO
                {
                    FeedbackId = feedback.FeedbackId,
                    Rating = feedback.Rate,
                    Message = feedback.Message,
                    CreatedAt = feedback.CreatedAt
                };

                return CreatedAtAction(nameof(Submit), new { id = feedbackDto.FeedbackId }, feedbackDto);
            }
            catch (VisitDoesntExistException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status404NotFound
                );
            }
            catch (VisitNotConfirmedException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (FeedbackForbiddenActionException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status403Forbidden
                );
            }
            catch (FeedbackAlreadyExistsException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status409Conflict
                );
            }
            catch (FeedbackInvalidRatingException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (FeedbackMessageTooLongException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (Exception e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        [HttpGet("{agentId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public ActionResult<IEnumerable<FeedbackListDTO>> List(int agentId)
        {
            try
            {
                var feedbacks = feedbackService.List(agentId);

                return Ok(feedbacks);
            }catch (AgentNotFoundException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status404NotFound
                 );
            }
            catch (Exception e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                 );
            }
        }
    }
}
