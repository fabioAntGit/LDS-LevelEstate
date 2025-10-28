using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public User User { get; set; }
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(150)")]
        [StringLength(150)]
        public string Message { get; set; }

        public Property Property { get; set; }
        public int PropertyId { get; set; }

        [Required]
        public DateTime SendDate { get; set; } = DateTime.Now;

        [Required]
        public bool IsRead { get; set; } = false;
    }
}
