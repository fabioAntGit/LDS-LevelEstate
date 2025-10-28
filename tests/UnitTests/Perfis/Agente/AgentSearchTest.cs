using API.Data;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Exceptions;
using API.Perfis.Agente.Services;
using API.RankingsBadges.Badges.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Perfis.Agente
{
    public class AgentSearchTest
    {
        private readonly LevelEstateContext context;
        private readonly AgentService agentService;

        public AgentSearchTest()
        {
            context = new LevelEstateContext(
                new DbContextOptionsBuilder<LevelEstateContext>()
                .UseInMemoryDatabase($"AgentSearchTestDB_{Guid.NewGuid()}")
                .Options
            );

            SeedFakeAgentsWithBadges();
            agentService = new AgentService(context);
        }

        [Fact]
        public void GetAgents_ReturnsAllVerifiedAgents()
        {
            var result = agentService.GetAgents(null, null, null);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetAgents_FilterByName_ReturnsCorrectAgent()
        {
            var result = agentService.GetAgents(null, null, "Bruno");

            Assert.Single(result);
            Assert.Equal("Bruno", result.First().FirstName);
        }

        [Fact]
        public void GetAgents_FilterByBadgePosition_ReturnsAgentsWithThatPosition()
        {
            var result = agentService.GetAgents(1, null, null);

            Assert.All(result, a => Assert.Contains(a.Badges, b => b.Position == 1));
        }

        [Fact]
        public void GetAgents_FilterByMonthly_ReturnsAgentsWithThatTypeOfRanking()
        {
            var result = agentService.GetAgents(null, TypeRanking.Monthly, null).ToList();

            Assert.Equal(2, result[0].AgentId);
        }

        [Fact]
        public void GetAgents_InvalidPosition_ThrowsException()
        {
            Assert.Throws<BadgePositionNotFoundException>(() => agentService.GetAgents(5, null, null));
        }

        [Fact]
        public void GetAgents_NoAgentsForFilters_ThrowsException()
        {
            Assert.Throws<AgentNotFoundException>(() => agentService.GetAgents(3, null, null));
        }

        [Fact]
        public void GetAgents_NoVerifiedAgents_ThrowsException()
        {
            context.Agents.RemoveRange(context.Agents);
            context.SaveChanges();

            Assert.Throws<AgentNotFoundException>(() => agentService.GetAgents(null, null, null));
        }

        private void SeedFakeAgentsWithBadges()
        {
            var agents = new List<Agent>
            {
                new Agent
                {
                    UserId = 1,
                    FirstName = "Ana",
                    LastName = "Silva",
                    Phone = "910000001",
                    Email = "ana@teste.com",
                    Password = "123456",
                    Role = Role.Agent,
                    AMICode = "AMI001",
                    ReferralCode = "REF001",
                    MonthlyPoints = 50,
                    AnnualPoints = 100,
                    AllMoneyEarned = 2000,
                    MoneyEarnedLastMonth = 300,
                    RegistrationDate = DateTime.UtcNow,
                    Status = UserStatus.Verified
                },
                new Agent
                {
                    UserId = 2,
                    FirstName = "Bruno",
                    LastName = "Costa",
                    Phone = "910000002",
                    Email = "bruno@teste.com",
                    Password = "123456",
                    Role = Role.Agent,
                    AMICode = "AMI002",
                    ReferralCode = "REF002",
                    MonthlyPoints = 70,
                    AnnualPoints = 300,
                    AllMoneyEarned = 5000,
                    MoneyEarnedLastMonth = 700,
                    RegistrationDate = DateTime.UtcNow,
                    Status = UserStatus.Verified
                },
                new Agent
                {
                    UserId = 3,
                    FirstName = "Carla",
                    LastName = "Ferreira",
                    Phone = "910000003",
                    Email = "carla@teste.com",
                    Password = "123456",
                    Role = Role.Agent,
                    AMICode = "AMI003",
                    ReferralCode = "REF003",
                    MonthlyPoints = 60,
                    AnnualPoints = 200,
                    AllMoneyEarned = 3500,
                    MoneyEarnedLastMonth = 500,
                    RegistrationDate = DateTime.UtcNow,
                    Status = UserStatus.Verified
                }
            };

            var badges = new List<Badge>
            {
                new Badge { BadgeId = 1, AgentId = 1, Position = 1, Month = 10, Year = 2025 },
                new Badge { BadgeId = 2, AgentId = 2, Position = 1, Month = 10, Year = 2025 },
                new Badge { BadgeId = 3, AgentId = 3, Position = 2, Month = 10, Year = 2025 }
            };

            context.Agents.AddRange(agents);
            context.Badges.AddRange(badges);
            context.SaveChanges();
        }
    }
}
