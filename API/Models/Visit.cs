using API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Visit
    {
        [Key]
        public int VisitId { get; set; }

        public Client Client { get; set; }
        public int ClientId { get; set; }

        public Agent Agent { get; set; }
        public int AgentId { get; set; }

        public Property Property { get; set; }
        public int PropertyId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public VisitStatus Status { get; set; } = VisitStatus.WaitingApproval;

        public Feedback? Feedback { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
