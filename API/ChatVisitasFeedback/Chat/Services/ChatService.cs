using API.ChatVisitasFeedback.Chat.DTOs;
using API.ChatVisitasFeedback.Chat.Exceptions;
using API.ChatVisitasFeedback.Chat.Interfaces;
using API.Data;
using API.Imoveis.Exceptions;

namespace API.ChatVisitasFeedback.Chat.Services
{
    public class ChatService : IChatService
    {
        private readonly LevelEstateContext context;
        public ChatService(LevelEstateContext context)
        {
            this.context = context;
        }

        public List<ChatDTO> List(int idUser)
        {
            return context.Chats
                .Where(c => c.ClientId == idUser || c.AgentId == idUser)
                .Select(c => new ChatDTO
                {
                    ChatId = c.ChatId,
                    PropertyId = c.Property.PropertyId,
                    Title = c.Property.Title,
                    ClientFirstName = c.Client.FirstName,
                    ClientLastName = c.Client.LastName,
                    AgentId = c.Client.UserId,
                    AgentFirstName = c.Agent.FirstName,
                    AgentLastName = c.Agent.LastName,
                    CreatedAt = c.CreatedAt,
                    ClientId = c.Client.UserId
                })
                .ToList();
        }

        public ChatDTO Create(int idClient, CreateChatDTO dto)
        {
            var property = context.Properties
                .Select(p => new { p.PropertyId, p.AgentId })
                .Where(p => p.PropertyId == dto.PropertyId)
                .SingleOrDefault();

            if (property == null)
            {
                throw new PropertyNotFoundException("Property was not found!");
            }

            var newChat = new Models.Chat
            {
                ClientId = idClient,
                AgentId = property.AgentId,
                PropertyId = property.PropertyId
            };

            context.Chats.Add(newChat);

            if (dto.Message != null && dto.Message.Trim() != "")
            {
                var newMessage = new Models.Message { ChatId = newChat.ChatId, SenderId = idClient, Content = dto.Message };
                context.Messages.Add(newMessage);
            }
            
            context.SaveChanges();

            return new ChatDTO
            {
                ChatId = newChat.ChatId,
                PropertyId = newChat.Property.PropertyId,
                Title = newChat.Property.Title,
                ClientFirstName = newChat.Client.FirstName,
                ClientLastName = newChat.Client.LastName,
                AgentId = newChat.Agent.UserId,
                AgentFirstName = newChat.Agent.FirstName,
                AgentLastName = newChat.Agent.LastName,
                CreatedAt = newChat.CreatedAt,
                ClientId = newChat.Client.UserId
            };
        }

        public ChatDTO Get(int idChat, int idUser)
        {
            var chat = context.Chats
                .Where(c => c.ChatId == idChat)
                .Select(c => new ChatDTO
                {
                    ChatId = c.ChatId,
                    PropertyId = c.Property.PropertyId,
                    Title = c.Property.Title,
                    ClientFirstName = c.Client.FirstName,
                    ClientLastName = c.Client.LastName,
                    AgentId = c.Agent.UserId,
                    AgentFirstName = c.Agent.FirstName,
                    AgentLastName = c.Agent.LastName,
                    CreatedAt = c.CreatedAt,
                    ClientId = c.Client.UserId
                })
                .SingleOrDefault();

            if (chat == null)
            {
                throw new ChatNotFoundException($"Chat with id {idChat} Not Found");
            }

            if (chat.AgentId != idUser && chat.ClientId != idUser)
            {
                throw new UnauthorizedChatAccessException($"User with id {idUser} is not authorized to access chat with id {idChat}");
            }

            return chat;
        }
    }
}
