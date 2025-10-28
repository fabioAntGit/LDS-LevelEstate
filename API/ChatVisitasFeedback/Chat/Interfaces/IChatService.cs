using API.ChatVisitasFeedback.Chat.DTOs;

namespace API.ChatVisitasFeedback.Chat.Interfaces
{
    public interface IChatService
    {
        public List<ChatDTO> List(int idUser);
        public ChatDTO Create(int idClient, CreateChatDTO dto);
        public ChatDTO Get(int idChat, int idUser);
    }
}
