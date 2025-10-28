using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        public Chat Chat { get; set; }
        public int ChatId { get; set; }

        public User Sender { get; set; }
        public int SenderId { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(300)")]
        [StringLength(300)]
        public string Content { get; set; }

        [Required]
        public DateTime SendDate { get; set; } = DateTime.UtcNow;
    }
}
