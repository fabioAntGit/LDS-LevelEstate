#nullable disable
using System.ComponentModel.DataAnnotations;

namespace API.ChatVisitasFeedback.Visitas.DTO
{
    public class CreateVisitDTO
    {
        [Required]
        public int PropertyId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        
    }
}
