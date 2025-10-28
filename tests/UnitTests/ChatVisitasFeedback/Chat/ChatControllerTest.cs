using API.ChatVisitasFeedback.Chat.Controllers;
using API.ChatVisitasFeedback.Chat.DTOs;
using API.ChatVisitasFeedback.Chat.Exceptions;
using API.ChatVisitasFeedback.Chat.Interfaces;
using API.Imoveis.Exceptions;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.ChatVisitasFeedback.Chat
{
    public class ChatControllerTest
    {
        private readonly ChatController chatController;
        private readonly IChatService chatService;
        private readonly IMessageService messageService;

        public ChatControllerTest()
        {
            chatService = A.Fake<IChatService>();
            messageService = A.Fake<IMessageService>();
            chatController = new ChatController(chatService, messageService);
        }

        [Fact]
        public void GetChats_UserWithNoChats_ReturnsOkEmptyList()
        {
            var expectedResult = new List<ChatDTO>();

            // Arrange
            A.CallTo(() => chatService.List(A<int>.Ignored)).Returns(expectedResult);

            // Act
            var result = chatController.GetChats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);

            var resultList = Assert.IsType<List<ChatDTO>>(okResult.Value);
            Assert.Empty(resultList);
        }

        [Fact]
        public void GetChats_UserWithNoChats_ReturnsOkNonEmptyList()
        {
            var expectedResult = new List<ChatDTO>
            {
                new ChatDTO
                {
                    ChatId = 1,
                    PropertyId = 101,
                    Title = "Sample Property",
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AgentId = 0,
                    AgentFirstName = "Agent",
                    AgentLastName = "Smith",
                    ClientId = 0
                },
                new ChatDTO
                {
                    ChatId = 2,
                    PropertyId = 101,
                    Title = "Sample Property",
                    ClientFirstName = "John",
                    ClientLastName = "Doe",
                    AgentId = 0,
                    AgentFirstName = "Agent",
                    AgentLastName = "Smith",
                    ClientId = 0
                }
            };

            // Arrange
            A.CallTo(() => chatService.List(A<int>.Ignored)).Returns(expectedResult);

            // Act
            var result = chatController.GetChats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);

            var resultList = Assert.IsType<List<ChatDTO>>(okResult.Value);
            Assert.Equal(resultList.Count, expectedResult.Count);

            Assert.Equal(expectedResult[0].ChatId, resultList[0].ChatId);
            Assert.Equal(expectedResult[1].ChatId, resultList[1].ChatId);
        }


        [Fact]
        public void GetChat_ValidChat_ReturnsOk()
        {
            var chatId = 1;
            var expectedChat = new ChatDTO
            {
                ChatId = chatId,
                PropertyId = 101,
                Title = "Sample Property",
                ClientFirstName = "John",
                ClientLastName = "Doe",
                AgentId = 0,
                AgentFirstName = "Agent",
                AgentLastName = "Smith",
                ClientId = 0
            };

            // Arrange
            A.CallTo(() => chatService.Get(chatId, A<int>.Ignored)).Returns(expectedChat);

            // Act
            var result = chatController.GetChat(chatId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);

            var resultChat = Assert.IsType<ChatDTO>(okResult.Value);
            Assert.Equal(expectedChat.ChatId, resultChat.ChatId);
        }

        [Fact]
        public void GetChat_InvalidChat_ReturnsNotFound()
        {
            var chatId = 999;

            // Arrange
            A.CallTo(() => chatService.Get(chatId, A<int>.Ignored)).Throws(new ChatNotFoundException("Chat Not Found"));

            // Act
            var result = chatController.GetChat(chatId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(objectResult.StatusCode, StatusCodes.Status404NotFound);
        }

        [Fact]
        public void GetChat_UnauthorizedAccess_ReturnsForbid()
        {
            var chatId = 1;
            // Arrange
            A.CallTo(() => chatService.Get(chatId, A<int>.Ignored)).Throws(new UnauthorizedChatAccessException("User is not authorized to access this chat"));
            // Act
            var result = chatController.GetChat(chatId);
            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(objectResult.StatusCode, StatusCodes.Status403Forbidden);
        }

        [Fact]
        public void CreateChat_ValidChat_ReturnsCreatedAtAction()
        {
            var createChatDto = new CreateChatDTO
            {
                PropertyId = 101,
                Message = "Hello"
            };
            var expectedChat = new ChatDTO
            {
                ChatId = 1,
                PropertyId = 101,
                Title = "Sample Property",
                ClientFirstName = "John",
                ClientLastName = "Doe",
                AgentId = 0,
                AgentFirstName = "Agent",
                AgentLastName = "Smith",
                ClientId = 0
            };

            // Arrange
            A.CallTo(() => chatService.Create(A<int>.Ignored, createChatDto)).Returns(expectedChat);

            // Act
            var result = chatController.CreateChat(createChatDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(createdAtActionResult.Value);

            var resultChat = Assert.IsType<ChatDTO>(createdAtActionResult.Value);
            Assert.Equal(expectedChat.ChatId, resultChat.ChatId);

        }

        [Fact]
        public void CreateChat_InvalidProperty_ReturnsNotFound()
        {
            var createChatDto = new CreateChatDTO
            {
                PropertyId = 999,
                Message = "Hello"
            };

            // Arrange
            A.CallTo(() => chatService.Create(A<int>.Ignored, createChatDto)).Throws(new PropertyNotFoundException("Property not found"));

            // Act
            var result = chatController.CreateChat(createChatDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(objectResult.StatusCode, StatusCodes.Status404NotFound);
        }

        [Fact]
        public void GetMessages_NoMessages_ReturnsOkEmptyList()
        {
            var chatId = 1;
            var expectedMessages = new List<MessageDTO>();

            // Arrange
            A.CallTo(() => messageService.List(chatId, A<int>.Ignored)).Returns(expectedMessages);

            // Act
            var result = chatController.GetMessages(chatId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);

            var resultList = Assert.IsType<List<MessageDTO>>(okResult.Value);
            Assert.Empty(resultList);
        }

        [Fact]
        public void GetMessages_WithMessages_ReturnsOkNonEmptyList()
        {
            var chatId = 1;
            var expectedMessages = new List<MessageDTO>
            {
                new MessageDTO
                {
                    MessageId = 1,
                    ChatId = chatId,
                    SenderId = 0,
                    Content = "Hello",
                    SendDate = DateTime.UtcNow
                },
                new MessageDTO
                {
                    MessageId = 2,
                    ChatId = chatId,
                    SenderId = 0,
                    Content = "Hi",
                    SendDate = DateTime.UtcNow
                }
            };

            // Arrange
            A.CallTo(() => messageService.List(chatId, A<int>.Ignored)).Returns(expectedMessages);

            // Act
            var result = chatController.GetMessages(chatId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);

            var resultList = Assert.IsType<List<MessageDTO>>(okResult.Value);
            Assert.Equal(resultList.Count, expectedMessages.Count);
        }

        [Fact]
        public void GetMessages_InvalidChat_ReturnsNotFound()
        {
            var chatId = 999;

            // Arrange
            A.CallTo(() => messageService.List(chatId, A<int>.Ignored)).Throws(new ChatNotFoundException("Chat Not Found"));

            // Act
            var result = chatController.GetMessages(chatId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(objectResult.StatusCode, StatusCodes.Status404NotFound);
        }

        [Fact]
        public void GetMessages_UnauthorizedAccess_ReturnsForbid()
        {
            var chatId = 1;

            // Arrange
            A.CallTo(() => messageService.List(chatId, A<int>.Ignored)).Throws(new UnauthorizedChatAccessException("User is not authorized to access this chat"));

            // Act
            var result = chatController.GetMessages(chatId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(objectResult.StatusCode, StatusCodes.Status403Forbidden);
        }

    }
}