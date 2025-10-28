using API.FavoritosNotificacoes.Notificacoes.DTOs;
using API.FavoritosNotificacoes.Notificacoes.Exceptions;
using API.FavoritosNotificacoes.Notificacoes.Interfaces;
using API.Imoveis.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.FavoritosNotificacoes.Notificacoes.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService notificationService;

        public NotificationController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<NotificationDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<NotificationDTO>> GetUserNotifications()
        {
            // AUTH NOT IMPLEMENTED YET
            int userId = 1;

            try
            {
                var notifications = notificationService.ListUserNotifications(userId);
                return Ok(notifications);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("read/{notificationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult MarkNotificationAsRead(int notificationId)
        {
            try
            {
                int userId = 1; // Auth not implemented yet
                var result = notificationService.ReadOneNotification(userId, notificationId);

                return NoContent();
            }
            catch (NotificationNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("read-all")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult MarkAllAsRead()
        {
            // AUTH NOT IMPLEMENTED YET
            int userId = 1;

            try
            {
                var result = notificationService.ReadAllNotifications(userId);

                return Ok(true);
            }
            catch (NotificationNotFoundException e)
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
