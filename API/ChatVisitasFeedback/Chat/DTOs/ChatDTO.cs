#nullable disable
using System.ComponentModel.DataAnnotations;

namespace API.ChatVisitasFeedback.Chat.DTOs
{
    public class ChatDTO
    {
        [Required]
        public int ChatId {  get; set; }
        [Required]
        public int PropertyId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public int ClientId { get; set; }
        [Required]
        public string ClientFirstName { get; set; }
        [Required]
        public string ClientLastName { get; set; }
        [Required]
        public int AgentId { get; set; }
        [Required]
        public string AgentFirstName { get; set; }
        [Required]
        public string AgentLastName { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
