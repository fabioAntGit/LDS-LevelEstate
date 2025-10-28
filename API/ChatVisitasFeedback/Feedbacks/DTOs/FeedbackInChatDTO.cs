#nullable disable
using System.ComponentModel.DataAnnotations;

namespace API.ChatVisitasFeedback.Feedbacks.DTOs
{
    public class FeedbackInChatDTO
    {
        [Required]
        public int FeedbackId { get; set; }

        [Required]
        public byte Rating { get; set; }

        public string? Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
