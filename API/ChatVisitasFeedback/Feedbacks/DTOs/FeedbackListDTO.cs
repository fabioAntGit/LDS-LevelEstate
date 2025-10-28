#nullable disable
using System.ComponentModel.DataAnnotations;

namespace API.ChatVisitasFeedback.Feedbacks.DTOs
{
    public class FeedbackListDTO
    {
        [Required]
        public int PropertyId { get; set; }

        [Required]
        public string PropertyTitle { get; set; }

        [Required]
        public int FeedbackAuthorId { get; set; }

        [Required]
        public string FeedbackAuthorFirstName { get; set; }

        [Required]
        public string FeedbackAuthorLastName { get; set; }

        [Required]
        public byte Rating { get; set; }

        public string? Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
