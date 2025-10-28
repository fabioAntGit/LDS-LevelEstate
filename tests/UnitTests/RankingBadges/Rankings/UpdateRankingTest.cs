using API.Data;
using API.Models;
using API.Models.Enums;
using API.RankingsBadges.Rankings.Exceptions;
using API.RankingsBadges.Rankings.Services;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.RankingBadges.Rankings
{
    public class UpdateRankingTest
    {
        private readonly LevelEstateContext context;
        private readonly RankingService rankingService;

        public UpdateRankingTest()
        {
            var options = new DbContextOptionsBuilder<LevelEstateContext>()
                .UseInMemoryDatabase($"RankingServiceDB_{Guid.NewGuid()}").Options;

            context = new LevelEstateContext(options);
            rankingService = new RankingService(context);

            SeedDatabase();
        }

        [Fact]
        public async Task Update_ShouldUpdatePointsOnBothRankings()
        {
            var result = await rankingService.Update(1, 50);

            Assert.True(result);

            var monthly = context.AgentRankings.First(ar => ar.AgentId == 1 && ar.Ranking.Type == TypeRanking.Monthly);
            var yearly = context.AgentRankings.First(ar => ar.AgentId == 1 && ar.Ranking.Type == TypeRanking.Yearly);

            Assert.Equal(150, monthly.Points);
            Assert.Equal(1050, yearly.Points); 
        }

        [Fact]
        public async Task Update_WhenAgentNotInRankings_ShouldReturnFalse()
        {
            var result = await rankingService.Update(999, 50);

            Assert.False(result);
        }

        [Fact]
        public async Task Update_WhenNoRankingsExist_ShouldThrowException()
        {
            context.Rankings.RemoveRange(context.Rankings);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<RankingNotFoundException>(() => rankingService.Update(1, 50));
        }

        [Fact]
        public async Task Update_WhenPointsNegative_ShouldThrowArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => rankingService.Update(1, -10));
        }

        private void SeedDatabase()
        {
            var agent = new Agent
            {
                FirstName = "Miguel",
                LastName = "Barros",
                Phone = "+351912345678",
                Email = "miguelBarros@test.pt",
                Password = "Strong123!",
                AMICode = "AMI12345",
                ReferralCode = "REF12345",
                MonthlyPoints = 100,
                AnnualPoints = 1000,
                AllMoneyEarned = 200,
                MoneyEarnedLastMonth = 0,
                RegistrationDate = DateTime.UtcNow,
                Status = UserStatus.Verified
            };
            context.Agents.Add(agent);

            var monthly = new Ranking { RankingId = 1, Year = 2025, Month = 10, Type = TypeRanking.Monthly };
            var yearly = new Ranking { RankingId = 2, Year = 2025, Type = TypeRanking.Yearly };

            context.Rankings.AddRange(monthly, yearly);

            context.AgentRankings.AddRange(
                new AgentRanking { AgentId = 1, RankingId = 1, Points = 100 },
                new AgentRanking { AgentId = 1, RankingId = 2, Points = 1000 }
            );

            context.SaveChanges();
        }
    }
}
