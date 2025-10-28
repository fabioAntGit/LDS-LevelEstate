namespace API.FavoritosNotificacoes.Notificacoes.DTOs
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }

        public string Message { get; set; }

        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }

        public DateTime SendDate { get; set; }
        public bool IsRead { get; set; }
    }
}
