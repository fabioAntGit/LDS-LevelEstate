using API.Data;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Exceptions;
using API.Perfis.Agente.Interfaces;
using API.Perfis.Agente.Services;
using API.Perfis.Cliente.Exceptions;
using API.TaskScheduler.HangFire.Jobs;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Perfis.Agente
{
    public class AgentServiceTest
    {
        private readonly LevelEstateContext context;
        private readonly AgentService agentService;

        public AgentServiceTest()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                       .UseInMemoryDatabase($"AgentServiceDB_{Guid.NewGuid()}").Options
            );

            agentService = new AgentService(context);
            CreateAgents();
            CreateClients();
        }

        [Fact]
        public async Task ConvertPoints_ReturnsConvertPointsCorrectly()
        {
            var agent = context.Agents.First(a => a.UserId == 1);
            var agentInicialAllMoneyEarned = agent.AllMoneyEarned;
            var agentInicialMonthlyPoints = agent.MonthlyPoints;

            var result = await agentService.ConvertPoints(false);

            Assert.True(result);
            Assert.Equal(0, agent.MonthlyPoints);
            Assert.Equal(agentInicialAllMoneyEarned + (agentInicialMonthlyPoints * 0.5), agent.AllMoneyEarned);
            Assert.Equal(agentInicialMonthlyPoints * 0.5, agent.MoneyEarnedLastMonth);
        }

        [Fact]
        public async Task ConvertPoints_WhenAnnual_ShouldResetAnnualPointsForAllAgents()
        {
            var result = await agentService.ConvertPoints(true);

            Assert.True(result);

            var agents = context.Agents.ToList();
            Assert.All(agents, a => Assert.Equal(0, a.AnnualPoints));
        }

        [Fact]
        public async Task ConvertPoints_WhenNoAgentsWithPoints_ShouldReturnFalse()
        {
            foreach (var a in context.Agents)
            {
                a.MonthlyPoints = 0;
            }
            await context.SaveChangesAsync();

            var result = await agentService.ConvertPoints(false);

            Assert.False(result);
        }

        [Fact]
        public void AddPoints_NegativePoints_InvalidPoints()
        {
            var points = -4;
            var agentId = context.Agents.Select(a => a.UserId).First();

            Assert.Throws<InvalidPointsException>(() => agentService.AddPoints(agentId, points));
        }

        [Fact]
        public void AddPoints_InvalidAgent_AgentNotFound()
        {
            var points = 5;
            var agentId = 900;

            Assert.Throws<AgentNotFoundException>(() => agentService.AddPoints(agentId, points));
        }

        [Fact]
        public void AddPoints_UnverifiedAgent_AgentNotVerified()
        {
            CreateUnverifiedAgent();

            var points = 5;
            var agent = context.Agents.First(a => a.Status == UserStatus.NonVerified);

            Assert.Throws<AgentNotVerifiedException>(() => agentService.AddPoints(agent.UserId, points));
        }

        [Fact]
        public void AddPoints_CorrectPointsAdded_True()
        {
            const int points = 5;

            var agents = context.Agents
                .Select(a => new { a.UserId, a.AnnualPoints, a.MonthlyPoints });

            foreach (var agent in agents)
            {
                var expectedMonthlyPoints = agent.MonthlyPoints + points;
                var expectedAnnualPoints = agent.AnnualPoints + points;

                var result = agentService.AddPoints(agent.UserId, points);

                var agentAfter = context.Agents
                    .Where(a => a.UserId == agent.UserId)
                    .Select(a => new { a.MonthlyPoints, a.AnnualPoints })
                    .First();

                Assert.True(result);
                Assert.Equal(expectedMonthlyPoints, agentAfter.MonthlyPoints);
                Assert.Equal(expectedAnnualPoints, agentAfter.AnnualPoints);
            }
        }

        [Fact]
        public void ApplyAgentRefferalPoints_InvalidClient_ClientNotFound()
        {
            var clientId = 900;

            Assert.Throws<ClientNotFoundException>(() => agentService.ApplyAgentRefferalPoints(clientId));
        }

        [Fact]
        public void ApplyAgentRefferalPoints_UnverifiedClient_ClientNotVerified()
        {
            var clientId = context.Clients
                .Where(c => c.Status == UserStatus.NonVerified)
                .Select(c => c.UserId)
                .First();

            Assert.Throws<ClientNotVerifiedException>(() => agentService.ApplyAgentRefferalPoints(clientId));
        }

        [Fact]
        public void ApplyAgentRefferalPoints_WrongRefferalCode_AgentNotFound()
        {
            var clientId = context.Clients
                .Where(c => c.ReferralCodeUsed.Equals("nao existe"))
                .Select(c => c.UserId)
                .First();

            Assert.Throws<AgentNotFoundException>(() => agentService.ApplyAgentRefferalPoints(clientId));
        }

        [Fact]
        public void ApplyAgentReferralPoints_NoCode_False()
        {
            var clientId = context.Clients
                .Where(c => c.ReferralCodeUsed == null)
                .Select(c => c.UserId)
                .First();

            var result = agentService.ApplyAgentRefferalPoints(clientId);

            Assert.False(result);
        }

        [Fact]
        public void ApplyAgentReferralPoints_ValidCode_True()
        {
            var clientId = context.Clients
                .Where(c => c.ReferralCodeUsed.Equals("REF12347"))
                .Select(c => c.UserId)
                .First();

            var result = agentService.ApplyAgentRefferalPoints(clientId);

            Assert.True(result);
        }

        private void CreateAgents()
        {
            var agents = new[]
            {
                new Agent
                {
                    FirstName = "Fabio",
                    LastName = "Antunes",
                    Phone = "+351912345678",
                    Email = "fabio.antunes@test.pt",
                    Password = "Password123!",
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
                },
                new Agent
                {
                    FirstName = "papaiCris",
                    LastName = "Ronaldo",
                    Phone = "+351912345689",
                    Email = "papaiCris.ronaldo@test.pt",
                    Password = "Password127!",
                    AMICode = "AMI12347",
                    ReferralCode = "REF12347",
                    MonthlyPoints = 0,
                    AnnualPoints = 123,
                    AllMoneyEarned = 323,
                    MoneyEarnedLastMonth = 0,
                    RegistrationDate = DateTime.UtcNow,
                    Status = UserStatus.Verified
                },
            };

            context.Agents.AddRange(agents);
            context.SaveChanges();
        }

        private void CreateUnverifiedAgent()
        {
            var agent = new Agent
            {
                FirstName = "cristianinho",
                LastName = "Ronaldo",
                Phone = "+351912345689",
                Email = "cristianinho.ronaldo@test.pt",
                Password = "Password127!",
                AMICode = "AMI12357",
                ReferralCode = "REF123478",
                MonthlyPoints = 0,
                AnnualPoints = 123,
                AllMoneyEarned = 323,
                MoneyEarnedLastMonth = 0,
                RegistrationDate = DateTime.UtcNow,
                Status = UserStatus.NonVerified
            };

            context.Agents.Add(agent);
            context.SaveChanges();
        }

        private void CreateClients()
        {
            var clients = new[]
            {
                new Client
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    Password = "Password127!",
                    ReferralCodeUsed = "REF12347",
                    Phone = "1233448945",
                    Status = UserStatus.Verified,
                    RegistrationDate = DateTime.UtcNow,
                },
                new Client
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    Password = "Password127!",
                    ReferralCodeUsed = null,
                    Phone = "1233448946",
                    Status = UserStatus.Verified,
                    RegistrationDate = DateTime.UtcNow,
                },
                new Client
                {
                    FirstName = "Jack",
                    LastName = "Doe",
                    Email = "jack.doe@example.com",
                    Password = "Password127!",
                    ReferralCodeUsed = "nao existe",
                    Phone = "1233448946",
                    Status = UserStatus.Verified,
                    RegistrationDate = DateTime.UtcNow,
                },
                new Client
                {
                    FirstName = "James",
                    LastName = "Doe",
                    Email = "james.doe@example.com",
                    Password = "Password127!",
                    ReferralCodeUsed = null,
                    Phone = "1233448946",
                    Status = UserStatus.NonVerified,
                    RegistrationDate = DateTime.UtcNow,
                }
            };

            context.Clients.AddRange(clients);
            context.SaveChanges();
        }
    }
}
