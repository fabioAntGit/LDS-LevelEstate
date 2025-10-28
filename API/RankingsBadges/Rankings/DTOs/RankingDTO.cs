using API.Models.Enums;

namespace API.RankingsBadges.Rankings.DTOs
{
    public class RankingDTO
    {
        public int AgentRankingId { get; set; }
        public int RankingId { get; set; }
        public IEnumerable<AgentRankingDTO> RankingList { get; set; }
        public short Year { get; set; }
        public byte? Month { get; set; }
        public TypeRanking Type { get; set; }
    }
}
