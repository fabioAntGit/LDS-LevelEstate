using API.ChatVisitasFeedback.Chat.Exceptions;
using API.ChatVisitasFeedback.Visitas.DTO;
using API.ChatVisitasFeedback.Visitas.DTOs;
using API.ChatVisitasFeedback.Visitas.Exceptions;
using API.ChatVisitasFeedback.Visitas.Interfaces;
using API.Imoveis.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.ChatVisitasFeedback.Visitas.Controllers
{
    [Route("api/visits")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService visitService;

        public VisitController(IVisitService visitService)
        {
            this.visitService = visitService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(VisitDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VisitDTO> RequestVisit([FromBody] CreateVisitDTO dto)
        {
            // AUTH NOT IMPLEMENTED YET
            var userId = 2;

            try
            {
                var visit = visitService.Request(userId, dto.PropertyId, dto.ScheduledDate);

                var visitDto = new VisitDTO
                {
                    VisitId = visit.VisitId,
                    Date = visit.Date,
                    Status = visit.Status.ToString()
                };

                return CreatedAtAction(nameof(RequestVisit), new { id = visitDto.VisitId }, visitDto);
            }
            catch (PropertyNotFoundException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status404NotFound
                 );
            }
            catch (AgentCannotCreateOwnPropertyVisitException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status403Forbidden
                 );
            }
            catch (VisitAlreadyExistsException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status409Conflict
                 );
            }
            catch (VisitDateTooSoonException e)
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

        [HttpPut("{visitId}/accept")]
        [ProducesResponseType(typeof(VisitDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VisitDTO> acceptVisit(int visitId)
        {
            // AUTH NOT IMPLEMENTED YET
            var userId = 1;

            try
            {
                var visitUpdated = visitService.Accept(visitId, userId);

                var visitDto = new VisitDTO
                {
                    VisitId = visitUpdated.VisitId,
                    Date = visitUpdated.Date,
                    Status = visitUpdated.Status.ToString()
                };

                return Ok(visitDto);
            }
            catch (VisitDoesntExistException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status404NotFound
                 );
            }
            catch (VisitForbiddenActionException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status403Forbidden
                 );
            }
            catch (VisitIsNotWaitingApprovalException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status409Conflict
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

        [HttpPut("{visitId}/reject")]
        [ProducesResponseType(typeof(VisitDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VisitDTO> rejectVisit(int visitId)
        {
            // AUTH NOT IMPLEMENTED YET
            var userId = 1;

            try
            {
                var visitUpdated = visitService.Reject(visitId, userId);

                var visitDto = new VisitDTO
                {
                    VisitId = visitUpdated.VisitId,
                    Date = visitUpdated.Date,
                    Status = visitUpdated.Status.ToString()
                };

                return Ok(visitDto);
            }
            catch (VisitDoesntExistException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status404NotFound
                 );
            }
            catch (VisitForbiddenActionException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status403Forbidden
                 );
            }
            catch (VisitIsNotWaitingApprovalException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status409Conflict
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

        [HttpPut("{visitId}/cancel")]
        [ProducesResponseType(typeof(VisitDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VisitDTO> cancelVisit(int visitId)
        {
            // AUTH NOT IMPLEMENTED YET
            var userId = 2;

            try
            {
                var visitUpdated = visitService.Cancel(visitId, userId);

                var visitDto = new VisitDTO
                {
                    VisitId = visitUpdated.VisitId,
                    Date = visitUpdated.Date,
                    Status = visitUpdated.Status.ToString()
                };

                return Ok(visitDto);

            }
            catch (VisitDoesntExistException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status404NotFound
                );
            }
            catch (VisitForbiddenActionException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status403Forbidden
                );
            }
            catch (VisitIsNotAcceptedOrWaitingApprovalException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status409Conflict
                );
            }
            catch (VisitCanceledAfterLimitException e)
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

        [HttpPut("{visitId}/confirm")]
        [ProducesResponseType(typeof(VisitDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VisitDTO> confirmVisit(int visitId)
        {
            // AUTH NOT IMPLEMENTED YET
            var userId = 1;

            try
            {
                var visitUpdated = visitService.Confirm(visitId, userId);

                var visitDto = new VisitDTO
                {
                    VisitId = visitUpdated.VisitId,
                    Date = visitUpdated.Date,
                    Status = visitUpdated.Status.ToString()
                };

                return Ok(visitDto);

            }
            catch (VisitDoesntExistException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status404NotFound
                );
            }
            catch (VisitForbiddenActionException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status403Forbidden
                );
            }
            catch (VisitIsNotAcceptedOrWaitingApprovalException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status409Conflict
                );
            }
            catch (VisitConfirmBeforeOccurredException e)
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
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal server error"
                );
            }


        }

        [HttpGet]
        [ProducesResponseType(typeof(VisitDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<VisitListDTO>> listVisits()
        {
            // AUTH NOT IMPLEMENTED YET
            var idUser = 1;

            try
            {
                var visits = visitService.List(idUser);
                return Ok(visits);
            }
            catch (Exception e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                 );
            }
        }

        [HttpGet("{chatId}")]
        [ProducesResponseType(typeof(VisitDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VisitDTO> getVisit(int chatId)
        {
            // AUTH NOT IMPLEMENTED YET
            var idUser = 2;

            try
            {
                var visit = visitService.Get(chatId);

                var visitDto = new VisitDTO
                {
                    VisitId = visit.VisitId,
                    Date = visit.Date,
                    Status = visit.Status.ToString(),
                    FeedbackRate = visit.Feedback != null ? visit.Feedback.Rate : (byte?)null,
                    FeedbackMessage = visit.Feedback != null ? visit.Feedback.Message : (string?)null,
                    FeedbackCreatedAt = visit.Feedback != null ? visit.Feedback.CreatedAt : (DateTime?)null
                };

                return Ok(visitDto);

            }
            catch (ChatNotFoundException e)
            {
                return Problem(
                    detail: e.Message,
                    statusCode: StatusCodes.Status404NotFound
                );
            }
            catch (VisitDoesntExistException e)
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
