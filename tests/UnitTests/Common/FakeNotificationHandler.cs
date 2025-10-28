using API.FavoritosNotificacoes.Notificacoes.DTOs;
using API.FavoritosNotificacoes.Notificacoes.Interfaces;
using Microsoft.AspNetCore.Http;

namespace UnitTests.Common
{
    public class FakeNotificationHandler : INotificationHandler
    {
        public List<NotificationDTO> SentNotifications { get; } = new();

        public Task SendNotificationAsync(int userId, NotificationDTO dto)
        {
            SentNotifications.Add(dto);
            return Task.CompletedTask;
        }

        public Task HandleConnectionAsync(HttpContext context)
        {
            return Task.CompletedTask;
        }
    }
}
