using API.Data;
using API.Models;
using API.Models.Enums;
using API.RankingsBadges.Rankings.DTOs;
using API.RankingsBadges.Rankings.Exceptions;
using API.RankingsBadges.Rankings.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.RankingsBadges.Rankings.Services
{
    public class RankingService : IRankingService
    {
        public readonly LevelEstateContext context;

        public RankingService(LevelEstateContext context)
        {
            this.context = context;
        }

        public async Task<RankingDTO> GetRankingOf(int? month, int year)
        {
            Ranking? ranking;
            var now = DateTime.UtcNow;

            if (year > now.Year || year < 2025)
                throw new RankingNotFoundException($"No ranking exists for {(month.HasValue ? $"month {month} of " : "")}{year}.");

            if (month.HasValue)
            {
                ranking = await context.Rankings.FirstOrDefaultAsync(r => r.Year == (short)year && r.Month == (byte)month && r.Type == TypeRanking.Monthly);
            }
            else
            {
                ranking = await context.Rankings.FirstOrDefaultAsync(r => r.Year == (short)year && r.Type == TypeRanking.Yearly);
            }

            if (ranking == null)
                throw new RankingNotFoundException($"No ranking exists for {(month.HasValue ? $"month {month} of " : "")}{year}.");

            var agentIds = await context.AgentRankings.Where(ar => ar.RankingId == ranking.RankingId).Select(ar => ar.AgentId).Distinct().ToListAsync();
            var agents = await context.Agents.Where(a => agentIds.Contains(a.UserId)).ToListAsync();

            var rankingList = await (from ar in context.AgentRankings join a in context.Agents on ar.AgentId equals a.UserId
                                                where ar.RankingId == ranking.RankingId
                                                orderby ar.Points descending
                                                select new AgentRankingDTO
                                                {
                                                    UserId = ar.AgentId,
                                                    FirstName = a.FirstName,
                                                     LastName = a.LastName,
                                                     Points = ar.Points
                                                }).ToListAsync();

            var rankingDTO = new RankingDTO
            {
                RankingId = ranking.RankingId,
                Year = ranking.Year,
                Month = ranking.Month,
                Type = ranking.Type,
                RankingList = rankingList
            };

            return rankingDTO;
        }

        public async Task<bool> Update(int idAgent, int points)
        {
            if (points < 0)
                throw new ArgumentException("Points cannot be negative.");

            var lastMonthlyRanking = await context.Rankings
                .Where(r => r.Type == TypeRanking.Monthly)
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .FirstOrDefaultAsync();

            var lastYearlyRanking = await context.Rankings
                .Where(r => r.Type == TypeRanking.Yearly)
                .OrderByDescending(r => r.Year)
                .FirstOrDefaultAsync();

            if (lastMonthlyRanking == null && lastYearlyRanking == null)
                throw new RankingNotFoundException("No rankings have been created yet.");

            bool updated = false;

            if (lastMonthlyRanking != null)
            {
                var agentMonthly = await context.AgentRankings
                    .FirstOrDefaultAsync(ar => ar.RankingId == lastMonthlyRanking.RankingId && ar.AgentId == idAgent);


                if (agentMonthly != null)
                {
                    agentMonthly.Points += points;
                    updated = true;
                }
            }

            if (lastYearlyRanking != null)
            {
                var agentYearly = await context.AgentRankings
                    .FirstOrDefaultAsync(ar => ar.RankingId == lastYearlyRanking.RankingId && ar.AgentId == idAgent);


                if (agentYearly != null)
                {
                    agentYearly.Points += points;
                    updated = true;
                }
            }

            if (updated)
            {
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task CreateRanking(bool yearly)
        {
            var now = DateTime.UtcNow;

            var monthlyRanking = new Ranking
            {
                Year = (short)now.Year,
                Month = (byte)now.Month,
                Type = TypeRanking.Monthly
            };

            context.Rankings.Add(monthlyRanking);

            if (yearly)
            {
                var yearlyRanking = new Ranking
                {
                    Year = (short)now.Year,
                    Type = TypeRanking.Yearly
                };

                context.Rankings.Add(yearlyRanking);
            }

            await context.SaveChangesAsync();
        }
    }
}