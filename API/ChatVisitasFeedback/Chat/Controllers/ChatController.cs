using API.ChatVisitasFeedback.Chat.DTOs;
using API.ChatVisitasFeedback.Chat.Exceptions;
using API.ChatVisitasFeedback.Chat.Interfaces;
using API.Imoveis.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.ChatVisitasFeedback.Chat.Controllers
{
    [Route("api/chats")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;
        private readonly IMessageService messageService;

        public ChatController(IChatService chatService, IMessageService messageService)
        {
            this.chatService = chatService;
            this.messageService = messageService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ChatDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public ActionResult<List<ChatDTO>> GetChats()
        {
            // AUTH NOT IMPLEMENTED YET
            int idUser = 0;
            try
            {
                var chats = chatService.List(idUser);
                return Ok(chats);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{chatId}")]
        [ProducesResponseType(typeof(ChatDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ChatDTO> GetChat(int chatId)
        {
            // AUTH NOT IMPLEMENTED YET
            int idUser = 0;

            try
            {
                var foundChat = chatService.Get(chatId, idUser);
                return Ok(foundChat);
            }
            catch (ChatNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (UnauthorizedChatAccessException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status403Forbidden);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ChatDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ChatDTO> CreateChat(CreateChatDTO dto)
        {
            // AUTH NOT IMPLEMENTED YET
            int idUser = 0;
            try
            {
                var newChat = chatService.Create(idUser, dto);
                return CreatedAtAction(nameof(CreateChat), new { id = newChat.ChatId }, newChat);
            }
            catch (PropertyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{chatId}/messages")]
        [ProducesResponseType(typeof(List<MessageDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<MessageDTO>> GetMessages(int chatId)
        {
            // AUTH NOT IMPLEMENTED YET
            int idUser = 0;
            try
            {
                var messages = messageService.List(chatId, idUser);
                return Ok(messages);
            }
            catch (ChatNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (UnauthorizedChatAccessException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status403Forbidden);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
