#nullable disable
using System.ComponentModel.DataAnnotations;

namespace API.ChatVisitasFeedback.Chat.DTOs
{
    public class MessageDTO
    {
        [Required]
        public int MessageId { get; set; }
        [Required]
        public int ChatId { get; set; }
        [Required]
        public int SenderId { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime SendDate { get; set; }
    }
}
