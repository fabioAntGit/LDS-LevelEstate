using API.Models;

namespace API.RankingsBadges.Badges.Interfaces
{
    public interface IBadgeService
    {
        public Task Create(bool annual);
    }
}
