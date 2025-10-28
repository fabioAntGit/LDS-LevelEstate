using API.Data;
using API.FavoritosNotificacoes.Notificacoes.DTOs;
using API.FavoritosNotificacoes.Notificacoes.Exceptions;
using API.FavoritosNotificacoes.Notificacoes.Interfaces;
using API.Imoveis.Exceptions;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.FavoritosNotificacoes.Notificacoes.Services
{
    public class NotificationService : INotificationService
    {
        private readonly LevelEstateContext context;
        private readonly INotificationHandler notificationHandler;

        public NotificationService(LevelEstateContext context, INotificationHandler notificationHandler)
        {
            this.context = context;
            this.notificationHandler = notificationHandler;
        }

        public List<NotificationDTO> ListUserNotifications(int userId)
        {
            return context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SendDate)
                .Select(n => new NotificationDTO
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    PropertyId = n.Property.PropertyId,
                    PropertyTitle = n.Property.Title,
                    SendDate = n.SendDate,
                    IsRead = n.IsRead
                })
                .ToList();
        }

        public bool ReadAllNotifications(int userId)
        {
            var notifications = context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            if (!notifications.Any())
                throw new NotificationNotFoundException("No unread notifications found for this user.");

            foreach (var n in notifications)
                n.IsRead = true;

            context.SaveChanges();
            return true;
        }

        public bool ReadOneNotification(int userId, int idNotification)
        {
            var notification = context.Notifications
                .FirstOrDefault(n => n.NotificationId == idNotification && n.UserId == userId && !n.IsRead);

            if (notification == null)
                throw new NotificationNotFoundException("Notification not found or already read.");

            notification.IsRead = true;
            context.SaveChanges();
            return true;
        }

        public async Task<bool> CreateAsync(int propertyId, string message)
        {
            var property = await context.Properties
                .Where(p => p.PropertyId == propertyId)
                .Select(p => new { p.PropertyId, p.Title })
                .SingleOrDefaultAsync();

            if (property == null)
                throw new PropertyNotFoundException("Property not found.");

            var clients = await context.Clients
                .Include(c => c.Favorite)
                .Where(c => c.Favorite.Any(f => f.PropertyId == propertyId))
                .Select(c => new { c.UserId })
                .ToListAsync();

            if (!clients.Any())
                return false;

            foreach (var client in clients)
            {
                var notification = new Notification
                {
                    UserId = client.UserId,
                    PropertyId = propertyId,
                    Message = message,
                    SendDate = DateTime.UtcNow
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync(); 

                var dto = new NotificationDTO
                {
                    NotificationId = notification.NotificationId,
                    Message = message,
                    PropertyId = propertyId,
                    PropertyTitle = property.Title,
                    SendDate = notification.SendDate,
                    IsRead = false
                };

                await notificationHandler.SendNotificationAsync(client.UserId, dto);
            }

            return true;
        }
    }
}
