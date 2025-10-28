using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class AgentRanking
    {
        [Key]
        public int AgentRankingId { get; set; }

        public Agent Agent { get; set; }
        public int AgentId { get; set; }

        public Ranking Ranking { get; set; }
        public int RankingId { get; set; }

        [Required]
        public int Points { get; set; }
    }
}
