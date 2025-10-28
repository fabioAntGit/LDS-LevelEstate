using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        public Visit Visit { get; set; }
        public int VisitId { get; set; }

        [Required]
        [Column(TypeName = "TINYINT")]
        public byte Rate { get; set; }

        [Column(TypeName = "NVARCHAR(150)")]
        [StringLength(150)]
        public string? Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
