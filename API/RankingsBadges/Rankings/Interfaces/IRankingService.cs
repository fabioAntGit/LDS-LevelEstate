using API.RankingsBadges.Rankings.DTOs;

namespace API.RankingsBadges.Rankings.Interfaces
{
    public interface IRankingService
    {
        public Task<bool> Update(int idAgent, int points);
        public Task<RankingDTO> GetRankingOf(int? month, int year);
        public Task CreateRanking(bool yearly);
    }
}
