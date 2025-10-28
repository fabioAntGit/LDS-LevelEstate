using API.ChatVisitasFeedback.Chat.DTOs;
using API.ChatVisitasFeedback.Chat.Exceptions;
using API.ChatVisitasFeedback.Chat.Interfaces;
using API.ChatVisitasFeedback.Chat.Services;
using API.Data;
using API.Imoveis.Exceptions;
using API.Models;
using API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.ChatVisitasFeedback.Chat
{
    public class ChatServiceTest
    {
        private readonly LevelEstateContext context;
        private readonly IChatService chatService;

        public ChatServiceTest()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                .UseInMemoryDatabase($"ChatServiceTestDB_{Guid.NewGuid()}").Options);

            SeedDatabase();
            chatService = new ChatService(context);
        }

        // TF21
        [Fact]
        public void Create_CreateChat_NewChat()
        {
            // Arrange
            var clientId = 100;
            var propertyId = 1;
            var agentId = 200;
            var dto = new CreateChatDTO
            {
                PropertyId = propertyId,
                Message = "Olá, estou interessado!"
            };
            var initialCount = context.Chats.Count();

            // Act
            var chat = chatService.Create(clientId, dto);
            var finalCount = context.Chats.Count();

            // Assert
            Assert.NotNull(chat);
            Assert.Equal(initialCount + 1, finalCount);
            Assert.Equal(clientId, chat.ClientId);
            Assert.Equal(agentId, chat.AgentId);
            Assert.Equal(propertyId, chat.PropertyId);
        }

        // TF21
        [Fact]
        public void Create_WhenPropertyNotFound_ThrowNotFound()
        {
            // Arrange
            var dto = new CreateChatDTO
            {
                PropertyId = 9999, // inexistente
                Message = "Mensagem"
            };

            // Act & Assert
            Assert.Throws<PropertyNotFoundException>(
                () => chatService.Create(100, dto)
            );
        }

        [Fact]
        public void List_Chats_ChatListForUser()
        {
            // Arrange
            var userId = 100;

            // Act
            var result = chatService.List(userId);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, c =>
                Assert.True(c.ClientId == userId || c.AgentId == userId));
        }

        [Fact]
        public void Get_WhenUserIsAuthorized_ChatWithId()
        {
            // Arrange
            var existingChatId = 1;
            var userId = 100;
            var propertyId = 1;

            // Act
            var result = chatService.Get(existingChatId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingChatId, result.ChatId);
            Assert.Equal(propertyId, result.PropertyId);
        }

        [Fact]
        public void Get_WhenChatNotFound_ThrowNotFound()
        {
            // Act & Assert
            Assert.Throws<ChatNotFoundException>(
                () => chatService.Get(9999, 100)
            );
        }

        [Fact]
        public void Get_WhenUserNotParticipant_ThrowUnauthorized()
        {
            // Arrange
            var chatId = 1;
            var unauthorizedUserId = 999;

            // Act & Assert
            Assert.Throws<UnauthorizedChatAccessException>(
                () => chatService.Get(chatId, unauthorizedUserId)
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

            context.Agents.Add(agent);
            context.Clients.Add(client);
            context.Properties.Add(property);
            context.Chats.Add(chat);
            context.SaveChanges();
        }
    }
}
