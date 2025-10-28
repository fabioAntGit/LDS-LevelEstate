using API.Data;
using API.FavoritosNotificacoes.Notificacoes.DTOs;
using API.FavoritosNotificacoes.Notificacoes.Interfaces;
using API.FavoritosNotificacoes.Notificacoes.Services;
using API.Imoveis.Exceptions;
using API.Models;
using API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using UnitTests.Common;

namespace UnitTests.FavoritosNotificacoes.Notificacoes
{
    public class NotificationTest
    {
        private readonly LevelEstateContext context;
        private readonly FakeNotificationHandler handler;
        private readonly NotificationService notificationService;

        public NotificationTest()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                .UseInMemoryDatabase($"NotificationTestDB_{Guid.NewGuid()}").Options);

            handler = new FakeNotificationHandler();
            SeedDatabase();
            notificationService = new NotificationService(context, handler);
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyExistsAndHasFavorites_CreatesNotifications()
        {
            // Arrange
            var propertyId = 1;
            var message = "Preço atualizado!";

            // Act
            var result = await notificationService.CreateAsync(propertyId, message);

            // Assert
            Assert.True(result);

            var notifications = context.Notifications
                .Where(n => n.PropertyId == propertyId && n.Message == message)
                .ToList();

            Assert.NotEmpty(notifications);
            Assert.All(notifications, n =>
            {
                Assert.Equal(message, n.Message);
                Assert.False(n.IsRead);
            });

            Assert.True(handler.SentNotifications.Any());
            Assert.Equal(message, handler.SentNotifications.First().Message);
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyHasNoFavorites_ReturnsFalse()
        {
            // Arrange
            var propertyWithoutFavoritesId = 2; 
            var message = "Preço alterado!";

            // Act
            var result = await notificationService.CreateAsync(propertyWithoutFavoritesId, message);

            // Assert
            Assert.False(result);
            Assert.Empty(handler.SentNotifications);
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyDoesNotExist_ThrowsPropertyNotFound()
        {
            // Arrange
            var invalidPropertyId = 999;
            var message = "Teste";

            // Act & Assert
            await Assert.ThrowsAsync<PropertyNotFoundException>(() =>
                notificationService.CreateAsync(invalidPropertyId, message)
            );
        }

        [Fact]
        public void ListUserNotifications_WhenUserHasNotifications_ReturnsList()
        {
            // Arrange
            var userId = 100;

            // Act
            var result = notificationService.ListUserNotifications(userId);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, n => Assert.Equal(userId, context.Notifications.Find(n.NotificationId).UserId));
        }

        [Fact]
        public void ReadAllNotifications_WhenUserHasUnread_UpdatesIsRead()
        {
            // Arrange
            var userId = 100;

            // Act
            var result = notificationService.ReadAllNotifications(userId);

            // Assert
            Assert.True(result);
            Assert.All(context.Notifications.Where(n => n.UserId == userId), n => Assert.True(n.IsRead));
        }

        [Fact]
        public void ReadOneNotification_WhenUnread_UpdatesToRead()
        {
            // Arrange
            var notification = context.Notifications.First(n => !n.IsRead);
            var userId = notification.UserId;

            // Act
            var result = notificationService.ReadOneNotification(userId, notification.NotificationId);

            // Assert
            Assert.True(result);
            var updated = context.Notifications.Find(notification.NotificationId);
            Assert.True(updated.IsRead);
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
                Status = UserStatus.Verified,
                Favorite = new List<Property>()
            };

            var propertyWithFavorites = new Property
            {
                PropertyId = 1,
                Title = "Apartamento Teste",
                Price = 150000,
                Agent = agent,
                Status = Status.Available,
                Topology = Topologies.Apartment,
                TypeOperation = TypeOperations.Sale,
                Street = "Rua A",
                PostalCode = "4000-001",
            };

            var propertyWithoutFavorites = new Property
            {
                PropertyId = 2,
                Title = "Casa Sem Favoritos",
                Price = 200000,
                Agent = agent,
                Status = Status.Available,
                Topology = Topologies.House,
                TypeOperation = TypeOperations.Sale,
                Street = "Rua B",
                PostalCode = "4000-002",
            };

            client.Favorite.Add(propertyWithFavorites);

            var notification = new Notification
            {
                UserId = client.UserId,
                PropertyId = propertyWithFavorites.PropertyId,
                Message = "Teste",
                SendDate = DateTime.UtcNow,
                IsRead = false
            };

            context.Agents.Add(agent);
            context.Clients.Add(client);
            context.Properties.AddRange(propertyWithFavorites, propertyWithoutFavorites);
            context.Notifications.Add(notification);
            context.SaveChanges();
        }

    }
}
