using API.ChatVisitasFeedback.Chat.DTOs;
using API.ChatVisitasFeedback.Chat.Interfaces;
using API.ChatVisitasFeedback.Chat.Exceptions;
using API.Data;
using API.Models;

namespace API.ChatVisitasFeedback.Chat.Services
{
    public class MessageService : IMessageService
    {
        private readonly LevelEstateContext context;
        public MessageService(LevelEstateContext context)
        {
            this.context = context;
        }

        public List<MessageDTO> List(int idChat, int idUser)
        {
            var chat = context.Chats
                .Where(c => c.ChatId == idChat)
                .Select(c => new
                {
                    c.AgentId,
                    c.ClientId,
                    Messages = c.Messages.Select(m => new MessageDTO
                    {
                        MessageId = m.MessageId,
                        ChatId = m.ChatId,
                        SenderId = m.SenderId,
                        Content = m.Content,
                        SendDate = m.SendDate
                    })
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

            return chat.Messages.ToList();
        }

        public MessageDTO Send(int idChat, int idSender, string content)
        {
            var chat = context.Chats
                .Select(c => new { c.ChatId, c.AgentId, c.ClientId })
                .Where(c => c.ChatId == idChat)
                .SingleOrDefault();

            if (chat == null)
            {
                throw new ChatNotFoundException($"Chat with id {idChat} Not Found");
            }

            if (chat.AgentId != idSender && chat.ClientId != idSender)
            {
                throw new UnauthorizedChatAccessException($"User with id {idSender} is not authorized to access chat with id {idChat}");
            }

            var message = new Message
            {
                ChatId = idChat,
                SenderId = idSender,
                Content = content,
            };
            context.Messages.Add(message);
            context.SaveChanges();

            return new MessageDTO
            {
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                MessageId = message.MessageId,
                SendDate = message.SendDate
            };
        }
    }
}