using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Badge
    {
        [Key]
        public int BadgeId { get; set; }

        public Agent Agent { get; set; }
        public int AgentId { get; set; }

        [Required]
        [Range(1, 3)]
        public byte Position { get; set; }

        [Range(1, 12)]
        public byte? Month { get; set; }

        [Required]
        [Range(1800, 2100)]
        public short Year { get; set; }
    }
}
