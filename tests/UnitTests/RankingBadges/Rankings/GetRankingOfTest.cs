using API.Data;
using API.Models;
using API.Models.Enums;
using API.RankingsBadges.Rankings.Exceptions;
using API.RankingsBadges.Rankings.Services;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.RankingBadges.Rankings
{
    public class GetRankingOfTest
    {
        private readonly LevelEstateContext context;
        private readonly RankingService rankingService;
        public GetRankingOfTest()
        {
            var options = new DbContextOptionsBuilder<LevelEstateContext>()
                .UseInMemoryDatabase($"RankingServiceRankingOfDB_{Guid.NewGuid()}").Options;

            context = new LevelEstateContext(options);
            rankingService = new RankingService(context);

            SeedDatabase();
        }

        [Fact]
        public async Task GetRankingOf_ShouldReturnMonthlyRanking_WhenExists()
        {
            var result = await rankingService.GetRankingOf(10, 2025);

            Assert.NotNull(result);
            Assert.Equal((byte)10, result.Month);
            Assert.Equal(2025, result.Year);
            Assert.Equal(TypeRanking.Monthly, result.Type);
            Assert.Equal(2, result.RankingList.Count());
            Assert.Equal(150, result.RankingList.First().Points);
        }

        [Fact]
        public async Task GetRankingOf_ShouldReturnYearlyRanking_WhenMonthNull()
        {
            var result = await rankingService.GetRankingOf(null, 2025);

            Assert.NotNull(result);
            Assert.Equal(2025, result.Year);
            Assert.Equal(TypeRanking.Yearly, result.Type);
        }

        [Fact]
        public async Task GetRankingOf_ShouldThrow_WhenYearBefore2025()
        {
            await Assert.ThrowsAsync<RankingNotFoundException>(() => rankingService.GetRankingOf(10, 2023));
        }

        [Fact]
        public async Task GetRankingOf_ShouldThrow_WhenRankingDoesNotExist()
        {
            await Assert.ThrowsAsync<RankingNotFoundException>(() => rankingService.GetRankingOf(12, 2030));
        }

        private void SeedDatabase()
        {
            var agents = new[]
            {
                new Agent
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
                },
                new Agent
                {
                    FirstName = "Rita",
                    LastName = "Santos",
                    Phone = "+351912345688",
                    Email = "rita.santos@test.pt",
                    Password = "Password124!",
                    AMICode = "AMI12346",
                    ReferralCode = "REF12346",
                    MonthlyPoints = 0,
                    AnnualPoints = 200,
                    AllMoneyEarned = 200,
                    MoneyEarnedLastMonth = 0,
                    RegistrationDate = DateTime.UtcNow,
                    Status = UserStatus.Verified
                }
            };

            context.Agents.AddRange(agents);
            context.SaveChanges();

            var monthly = new Ranking { RankingId = 1, Year = 2025, Month = 10, Type = TypeRanking.Monthly };
            var yearly = new Ranking { RankingId = 2, Year = 2025, Type = TypeRanking.Yearly };

            context.Rankings.AddRange(monthly, yearly);

            context.AgentRankings.AddRange(
        new AgentRanking { RankingId = 1, AgentId = 1, Points = 150 },
                new AgentRanking { RankingId = 1, AgentId = 2, Points = 80 },
                new AgentRanking { RankingId = 2, AgentId = 1, Points = 200 },
                new AgentRanking { RankingId = 2, AgentId = 2, Points = 120 }
            );

            context.SaveChanges();
        }
    }
}
