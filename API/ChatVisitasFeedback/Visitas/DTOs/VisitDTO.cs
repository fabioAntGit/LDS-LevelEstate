#nullable disable
using System.ComponentModel.DataAnnotations;

namespace API.ChatVisitasFeedback.Visitas.DTOs
{
    public class VisitDTO
    {
        [Required]
        public int VisitId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; } = null!;

        public byte? FeedbackRate { get; set; }

        public string? FeedbackMessage { get; set; }

        public DateTime? FeedbackCreatedAt { get; set; }
    }
}
