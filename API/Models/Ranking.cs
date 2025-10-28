using System.ComponentModel.DataAnnotations;
using API.Models.Enums;



namespace API.Models
{
    public class Ranking
    {
        [Key]
        public int RankingId { get; set; }

        public ICollection<AgentRanking> RankingList { get; set; }

        [Required]
        public short Year { get; set; }

        [Range(1, 12)]
        public byte? Month { get; set; }

        [Required]
        public TypeRanking Type { get; set; }
    }
}
