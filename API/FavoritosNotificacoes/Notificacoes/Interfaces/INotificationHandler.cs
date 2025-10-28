using API.FavoritosNotificacoes.Notificacoes.DTOs;

namespace API.FavoritosNotificacoes.Notificacoes.Interfaces
{
    public interface INotificationHandler
    {
        Task SendNotificationAsync(int userId, NotificationDTO dto);
        Task HandleConnectionAsync(HttpContext context);
    }
}
