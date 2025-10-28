using API.FavoritosNotificacoes.Notificacoes.DTOs;

namespace API.FavoritosNotificacoes.Notificacoes.Interfaces
{
    public interface INotificationService
    {
        public List<NotificationDTO> ListUserNotifications(int UserId);
        public bool ReadAllNotifications(int UserId);
        public bool ReadOneNotification(int userId, int IdNotification);
        public Task<bool> CreateAsync(int IdProperty, string message);

    }
}
