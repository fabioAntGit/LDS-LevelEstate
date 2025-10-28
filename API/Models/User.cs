using API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Column(TypeName = "NVARCHAR(500)")]
        [StringLength(500)]
        public string? PfpImage { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(50)")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(50)")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(20)")]
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(320)")]
        [StringLength(320)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(100)")]
        [StringLength(100)]
        public string Password { get; set; }

        public ICollection<Visit> Visits { get; set; }

        public ICollection<Chat> Chats { get; set; }

        public ICollection<Notification> Notifications { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public UserStatus Status { get; set; } = UserStatus.NonVerified;

        [Required]
        public Role Role { get; set; }

    }
}
