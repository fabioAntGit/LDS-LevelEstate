#nullable disable
using System.ComponentModel.DataAnnotations;

namespace API.ChatVisitasFeedback.Chat.DTOs
{
    public class CreateChatDTO
    {
        [Required]
        public int PropertyId { get; set; }
        public string? Message { get; set; }
    }
}
