#nullable disable
using System.ComponentModel.DataAnnotations;

namespace API.ChatVisitasFeedback.Feedbacks.DTOs
{
    public class CreateFeedbackDTO
    {
        [Required]
        public int VisitId { get; set; }

        [Required]
        public byte Rating { get; set; }

        public string? Message { get; set; }

    }
}