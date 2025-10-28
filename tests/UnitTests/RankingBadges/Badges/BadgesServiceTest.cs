using API.Data;
using API.Models;
using API.Models.Enums;
using API.RankingsBadges.Badges.Services;
using API.RankingsBadges.Rankings.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.RankingBadges.Badges
{
    public class BadgesServiceTest
    {
        private readonly LevelEstateContext context;
        private readonly BadgeService badgeService;

        public BadgesServiceTest()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                       .UseInMemoryDatabase($"BadgesServiceTestDB_{Guid.NewGuid()}").Options
            );

            badgeService = new BadgeService(context);
        }

        [Fact]
        public async Task Create_MonthlyRankingExists_CreatesMonthlyBadges()
        {

            await CreateAgents();
            var ranking = await CreateMonthlyRanking((short)DateTime.UtcNow.Year, (byte)DateTime.UtcNow.AddMonths(-1).Month);
            await CreateAgentsWithDiffPositions(ranking.RankingId);

            await badgeService.Create(false);

            var badges = await context.Badges.OrderByDescending(r => r.Position).ToListAsync();
            Assert.Equal(3, badges.Count);
            Assert.Equal(3, badges.Find(r => r.AgentId == 3).Position);
            Assert.Equal(2, badges.Find(r => r.AgentId == 2).Position);
            Assert.Equal(1, badges.Find(r => r.AgentId == 1).Position);
        }

        [Fact]
        public async Task Create_AnnualRankingExists_CreatesYearlyBadges()
        {
            await CreateAgents();
            var ranking = await CreateAnnualRanking((short)DateTime.UtcNow.AddYears(-1).Year);
            await CreateMonthlyRanking((short)DateTime.UtcNow.Year, (byte)DateTime.UtcNow.AddMonths(-1).Month);
            await CreateAgentsWithDiffPositions(ranking.RankingId);

            await badgeService.Create(true);

            var badges = await context.Badges.OrderByDescending(r => r.Position).ToListAsync();

            Assert.NotEmpty(badges);
            Assert.Equal(3, badges.Count);
            Assert.Equal(3, badges.Find(r => r.AgentId == 3).Position);
            Assert.Equal(2, badges.Find(r => r.AgentId == 2).Position);
            Assert.Equal(1, badges.Find(r => r.AgentId == 1).Position);
        }

        [Fact]
        public async Task Create_MonthlyRankingWithDraws_AssignsBadgesCorrectly()
        {
            await CreateAgents();
            var ranking = await CreateAnnualRanking((short)DateTime.UtcNow.AddYears(-1).Year);
            await CreateMonthlyRanking((short)DateTime.UtcNow.Year, (byte)DateTime.UtcNow.AddMonths(-1).Month);
            await CreateAgentsWithDrawPositions(ranking.RankingId);

            await badgeService.Create(true);

            var badges = await context.Badges.OrderByDescending(r => r.Position).ToListAsync();

            Assert.NotEmpty(badges);
            Assert.Equal(6, badges.Count);
            Assert.Equal(3, badges.Find(r => r.AgentId == 6).Position);
            Assert.Equal(2, badges.Find(r => r.AgentId == 5).Position);
            Assert.Equal(2, badges.Find(r => r.AgentId == 4).Position);
            Assert.Equal(2, badges.Find(r => r.AgentId == 3).Position);
            Assert.Equal(1, badges.Find(r => r.AgentId == 2).Position);
            Assert.Equal(1, badges.Find(r => r.AgentId == 1).Position);
        }

        [Fact]
        public async Task Create_AnnualRankingWithDraws_AssignsBadgesCorrectly()
        {
            await CreateAgents();
            var ranking = await CreateMonthlyRanking((short)DateTime.UtcNow.Year, (byte)DateTime.UtcNow.AddMonths(-1).Month);
            await CreateAgentsWithDrawPositions(ranking.RankingId);

            await badgeService.Create(false);

            var badges = await context.Badges.OrderByDescending(r => r.Position).ToListAsync();

            Assert.NotEmpty(badges);
            Assert.Equal(6, badges.Count);
            Assert.Equal(3, badges.Find(r => r.AgentId == 6).Position);
            Assert.Equal(2, badges.Find(r => r.AgentId == 5).Position);
            Assert.Equal(2, badges.Find(r => r.AgentId == 4).Position);
            Assert.Equal(2, badges.Find(r => r.AgentId == 3).Position);
            Assert.Equal(1, badges.Find(r => r.AgentId == 2).Position);
            Assert.Equal(1, badges.Find(r => r.AgentId == 1).Position);
        }

        [Fact]
        public async Task Create_NoMonthlyRanking_ThrowsLastMonthRankingNotFoundException()
        {
            await Assert.ThrowsAsync<LastMonthRankingNotFoundException>(() =>
                badgeService.Create(false)
            );
        }

        [Fact]
        public async Task Create_NoAnnualRanking_ThrowsLastYearRankingNotFoundException()
        {
            await Assert.ThrowsAsync<LastYearRankingNotFoundException>(() =>
                badgeService.Create(true)
            );
        }

        private async Task<Ranking> CreateMonthlyRanking(short year, byte month)
        {
            var ranking = new Ranking
            {
                Year = year,
                Month = month,
                Type = TypeRanking.Monthly
            };

            await context.Rankings.AddAsync(ranking);
            await context.SaveChangesAsync();


            return ranking;
        }

        private async Task<Ranking> CreateAnnualRanking(short year)
        {
            var ranking = new Ranking
            {
                Year = year,
                Month = null,
                Type = TypeRanking.Yearly
            };

            await context.Rankings.AddAsync(ranking);
            await context.SaveChangesAsync();

            return ranking;
        }

        private async Task<List<AgentRanking>> CreateAgentsWithDiffPositions(int rankingId)
        {

            var agents = new List<AgentRanking>
            {
                new AgentRanking { AgentId = 1, RankingId = rankingId, Points = 1200 },
                new AgentRanking { AgentId = 2, RankingId = rankingId, Points = 1100 },
                new AgentRanking { AgentId = 3, RankingId = rankingId, Points = 1000 },
                new AgentRanking { AgentId = 4, RankingId = rankingId, Points = 900 },
                new AgentRanking { AgentId = 5, RankingId = rankingId, Points = 800 },
                new AgentRanking { AgentId = 6, RankingId = rankingId, Points = 700 },
                new AgentRanking { AgentId = 7, RankingId = rankingId, Points = 600 },
                new AgentRanking { AgentId = 8, RankingId = rankingId, Points = 500 },
                new AgentRanking { AgentId = 9, RankingId = rankingId, Points = 400 }
            };

            await context.AgentRankings.AddRangeAsync(agents);
            await context.SaveChangesAsync();

            return agents;
        }

        private async Task<List<AgentRanking>> CreateAgentsWithDrawPositions(int rankingId)
        {

            var agents = new List<AgentRanking>
            {
                new AgentRanking { AgentId = 1, RankingId = rankingId, Points = 1200 },
                new AgentRanking { AgentId = 2, RankingId = rankingId, Points = 1200 },
                new AgentRanking { AgentId = 3, RankingId = rankingId, Points = 1000 },
                new AgentRanking { AgentId = 4, RankingId = rankingId, Points = 1000 },
                new AgentRanking { AgentId = 5, RankingId = rankingId, Points = 1000 },
                new AgentRanking { AgentId = 6, RankingId = rankingId, Points = 900 },
                new AgentRanking { AgentId = 7, RankingId = rankingId, Points = 800 },
                new AgentRanking { AgentId = 8, RankingId = rankingId, Points = 700 },
                new AgentRanking { AgentId = 9, RankingId = rankingId, Points = 600 }
            };

            await context.AgentRankings.AddRangeAsync(agents);
            await context.SaveChangesAsync();

            return agents;
        }

        private async Task CreateAgents()
        {

            var agents = new List<Agent>();

            for (int i = 0; i < 9; i++)
            {
                agents.Append(new Agent
                {
                    FirstName = "Fabio" + i + 1,
                    LastName = "Antunes" + i + 1,
                    Phone = "+35191234567" + i + 1,
                    Email = "fabio.antunes" + i + 1 + "@test.pt",
                    Password = "Password123!" + i + 1,
                    AMICode = "AMI1234" + i + 1,
                    ReferralCode = "REF1234" + i + 1,
                    MonthlyPoints = 100,
                    AnnualPoints = 1000,
                    AllMoneyEarned = 200,
                    MoneyEarnedLastMonth = 0,
                    RegistrationDate = DateTime.UtcNow,
                    Status = UserStatus.Verified
                });
            }

            await context.Agents.AddRangeAsync(agents);
            await context.SaveChangesAsync();
        }
    }
}
