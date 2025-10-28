using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Chat
    {
        [Key]
        public int ChatId { get; set; }

        public Agent Agent { get; set; }
        public int AgentId { get; set; }

        public Client Client { get; set; }
        public int ClientId { get; set; }

        public Property Property { get; set; }
        public int PropertyId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Message> Messages { get; set; }
    }
}
