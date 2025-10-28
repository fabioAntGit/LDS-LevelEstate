#nullable disable
using System.ComponentModel.DataAnnotations;

namespace API.ChatVisitasFeedback.Chat.DTOs
{
    public class SendMessageDTO
    {
        [Required]
        public string Content { get; set; }
    }
}
