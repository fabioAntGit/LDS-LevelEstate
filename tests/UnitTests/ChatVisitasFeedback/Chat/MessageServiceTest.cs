using API.ChatVisitasFeedback.Chat.Exceptions;
using API.ChatVisitasFeedback.Chat.Interfaces;
using API.ChatVisitasFeedback.Chat.Services;
using API.Data;
using API.Models;
using API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.ChatVisitasFeedback.Chat
{
    public class MessageServiceTest
    {
        private readonly LevelEstateContext context;
        private readonly IMessageService messageService;

        public MessageServiceTest()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                .UseInMemoryDatabase($"MessageServiceTestDB_{Guid.NewGuid()}").Options);

            SeedDatabase();
            messageService = new MessageService(context);
        }


        // TF20
        [Fact]
        public void Send_SendMessage_True()
        {
            // Arrange
            var chatId = 1;
            var senderId = 100;
            var initialMessages = 2;
            var content = "Mensagem nova";

            // Act
            var message = messageService.Send(chatId, senderId, content);
            var messagesNumber = context.Messages.Count(m => m.ChatId == chatId);

            // Assert
            Assert.NotNull(message);
            Assert.Equal(initialMessages + 1, messagesNumber);
        }

        // TF20
        [Fact]
        public void Send_WhenChatNotFound_ThrowChatNotFound()
        {
            // Act & Assert
            Assert.Throws<ChatNotFoundException>(
                () => messageService.Send(9999, 100, "Mensagem")
            );
        }

        // TF20
        [Fact]
        public void Send_WhenUserNotInChat_ThrowUnauthorized()
        {
            // Arrange
            var chatId = 1;
            var unauthorizedUser = 999;

            // Act & Assert
            Assert.Throws<UnauthorizedChatAccessException>(
                () => messageService.Send(chatId, unauthorizedUser, "Tentativa não autorizada")
            );
        }


        [Fact]
        public void List_ListMessagesOfChat_ListOfMessagesOfSpecificChat()
        {
            // Arrange
            var chatId = 1;
            var userId = 100;

            // Act
            var messages = messageService.List(chatId, userId);

            // Assert
            Assert.NotEmpty(messages);
            Assert.All(messages, m => Assert.Equal(chatId, m.ChatId));
        }


        [Fact]
        public void List_WhenChatNotFound_ThrowChatNotFound()
        {
            // Act & Assert
            Assert.Throws<ChatNotFoundException>(
                () => messageService.List(9999, 100)
            );
        }


        [Fact]
        public void List_WhenUserNotInChat_ThrowUnauthorized()
        {
            // Arrange
            var chatId = 1;
            var unauthorizedUser = 999;

            // Act & Assert
            Assert.Throws<UnauthorizedChatAccessException>(
                () => messageService.List(chatId, unauthorizedUser)
            );
        }

        private void SeedDatabase()
        {
            var agent = new Agent
            {
                UserId = 200,
                FirstName = "Agente",
                LastName = "Teste",
                Email = "agente@teste.com",
                Phone = "910000001",
                Password = "123456",
                Role = Role.Agent,
                AMICode = "AMI001",
                ReferralCode = "REF001",
                Status = UserStatus.Verified
            };

            var client = new Client
            {
                UserId = 100,
                FirstName = "Cliente",
                LastName = "Teste",
                Email = "cliente@teste.com",
                Phone = "920000001",
                Password = "123456",
                Role = Role.Client,
                Status = UserStatus.Verified
            };

            var property = new Property
            {
                PropertyId = 1,
                Title = "Apartamento Teste",
                Price = 150000,
                Agent = agent,
                Status = Status.Available,
                Topology = Topologies.Apartment,
                NumberOfFloors = 2,
                TypeOperation = TypeOperations.Sale,
                Bathrooms = 2,
                Bedrooms = 2,
                AirConditioning = false,
                Balcony = true,
                Area = 2500,
                Description = "Description",
                Pictures = new List<Picture>
                    {
                        new Picture { PictureId = 1, ImagePath = "/img/property.jpg" }
                    },
                HasGarage = true,
                HasElevator = false,
                Furnished = false,
                PostalCode = "",
                Street = ""
            };

            var chat = new API.Models.Chat
            {
                ChatId = 1,
                Agent = agent,
                Client = client,
                Property = property,
            };

            var messages = new List<Message>
            {
                new Message { Chat = chat, SenderId = client.UserId, Content = "Mensagem A" },
                new Message { Chat = chat, SenderId = agent.UserId, Content = "Mensagem B" }
            };

            context.Agents.Add(agent);
            context.Clients.Add(client);
            context.Properties.Add(property);
            context.Chats.Add(chat);
            context.Messages.AddRange(messages);
            context.SaveChanges();
        }
    }
}
