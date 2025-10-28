using API.Data;
using API.Models;
using API.RankingsBadges.Badges.Interfaces;
using API.RankingsBadges.Rankings.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace API.RankingsBadges.Badges.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly LevelEstateContext context;

        public BadgeService(LevelEstateContext context)
        {
            this.context = context;
        }
        public async Task Create(bool annual)
        {
            if (annual)
            {
                var (top3Year, distinctPointsYear) = await GetLastYearTop3();
                var badgesYear = new List<Badge>();

                foreach (var agent in top3Year)
                {
                    byte position = (byte)(distinctPointsYear.IndexOf(agent.Points) + 1);

                    badgesYear.Add(new Badge
                    {
                        AgentId = agent.AgentId,
                        Position = position,
                        Month = null,
                        Year = agent.Ranking.Year
                    });
                }

                await context.AddRangeAsync(badgesYear);
            }

            var badgesMonth = new List<Badge>();
            var (top3, distinctPoints) = await GetLastMonthTop3();

            foreach (var agent in top3)
            {
                byte position = (byte)(distinctPoints.IndexOf(agent.Points) + 1);

                badgesMonth.Add(new Badge
                {
                    AgentId = agent.AgentId,
                    Position = position,
                    Month = agent.Ranking.Month,
                    Year = agent.Ranking.Year
                });
            }

            await context.AddRangeAsync(badgesMonth);
            await context.SaveChangesAsync();
            
        }

        private async Task<(List<AgentRanking> Top3Agents, List<int> DistinctPoints)> GetLastMonthTop3()
        {
            var now = DateTime.UtcNow;
            var lastMonth = now.Month - 1;
            var year = now.Year;

            var lastMonthRanking = await context.Rankings.Where(r => r.Month == lastMonth && r.Year == year).FirstOrDefaultAsync();

            if (lastMonthRanking == null)
            {
                throw new LastMonthRankingNotFoundException("There are no rankings for the last month");
            }

            var agentsOrdered = await context.AgentRankings
                .Where(r => r.RankingId == lastMonthRanking.RankingId)
                .Include(r => r.Ranking)
                .OrderByDescending(r => r.Points)
                .ToListAsync();

            var distinctPoints = agentsOrdered
                .Select(a => a.Points)
                .Distinct()
                .OrderByDescending(p => p)
                .Take(3)
                .ToList();

            var thirdPlacePointsMonth = distinctPoints.ElementAtOrDefault(2);

            var top3AgentsLastMonthRanking = agentsOrdered
                .Where(r => r.Points >= thirdPlacePointsMonth)
                .ToList();

            return (top3AgentsLastMonthRanking, distinctPoints);
        }

        private async Task<(List<AgentRanking> Top3Agents, List<int> DistinctPoints)> GetLastYearTop3()
        {
            var now = DateTime.UtcNow;
            var lastYear = now.Year - 1;

            var lastYearRanking = await context.Rankings.Where(r => r.Month == null && r.Year == lastYear).FirstOrDefaultAsync();

            if (lastYearRanking == null)
            {
                throw new LastYearRankingNotFoundException("There are no rankings for the last year");
            }

            var agentsOrdered = await context.AgentRankings
                .Where(r => r.RankingId == lastYearRanking.RankingId)
                .Include(r => r.Ranking)
                .OrderByDescending(r => r.Points)
                .ToListAsync();

            var distinctPoints = agentsOrdered
                .Select(a => a.Points)
                .Distinct()
                .OrderByDescending(p => p)
                .Take(3)
                .ToList();

            var thirdPlacePointsYear = distinctPoints.ElementAtOrDefault(2);

            var top3AgentsLastYearRanking = agentsOrdered
                .Where(r => r.Points >= thirdPlacePointsYear)
                .ToList();

            return (top3AgentsLastYearRanking, distinctPoints);
        }
    }
}