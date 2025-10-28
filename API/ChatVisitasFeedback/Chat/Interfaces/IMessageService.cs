using API.ChatVisitasFeedback.Chat.DTOs;

namespace API.ChatVisitasFeedback.Chat.Interfaces
{
    public interface IMessageService
    {
        public List<MessageDTO> List(int idChat, int idUser);
        public MessageDTO Send(int idChat, int idSender, string content);
    }
}
