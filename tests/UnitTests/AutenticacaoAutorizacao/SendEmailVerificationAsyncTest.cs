using API.AutenticacaoAutorizacao.Services;
using API.Data;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Interfaces;
using API.Perfis.Agente.Services;
using API.Perfis.Cliente.Exceptions;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UnitTests.AutenticacaoAutorizacao
{
    public class SendEmailVerificationAsyncTest
    {
        private readonly LevelEstateContext context;
        private readonly AuthService authService;
        private readonly IAgentService agentService;
        private readonly Client client;
        private readonly IConfiguration config;

        public SendEmailVerificationAsyncTest()
        {
            context = new LevelEstateContext(
                new DbContextOptionsBuilder<LevelEstateContext>()
                    .UseInMemoryDatabase($"SendEmailVerificationAsyncTestDB_{Guid.NewGuid()}")
                    .Options
            );

            Env.TraversePath().Load();
            config = new ConfigurationBuilder()
            .AddEnvironmentVariables("")
            .Build();

            agentService = new AgentService(context);
            authService = new AuthService(context, agentService, config);
            client = CreateClient();
        }

        [Fact]
        public async Task SendEmailVerificationAsync_WhenUserExistsAndNotVerified_ReturnValidJwt()
        {
            var token = await authService.SendEmailVerificationAsync(client.Email);

            Assert.NotNull(token);
            Assert.NotEmpty(token);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var emailInToken = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            Assert.Equal(client.Email, emailInToken);
            Assert.InRange(jwt.ValidTo - jwt.ValidFrom, TimeSpan.FromMinutes(29), TimeSpan.FromMinutes(31));
        }

        [Fact]
        public async Task SendEmailVerificationAsync_WhenUserDoesNotExist_ThrowClientNotFoundException()
        {
            var emailDoesNotExist = "test.notexist@exemple.com";
                
            await Assert.ThrowsAsync<ClientNotFoundException>(async () =>
            {
                await authService.SendEmailVerificationAsync(emailDoesNotExist);
            });
        }

        [Fact]
        public async Task SendEmailVerificationAsync_ShouldThrowClientAlreadyVerifiedException_WhenUserIsVerified()
        {

            client.Status = UserStatus.Verified;
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ClientAlreadyVerifiedException>(async () =>
            {
                await authService.SendEmailVerificationAsync(client.Email);
            });
        }

        private Client CreateClient()
        {
            var client = new Client
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "Password127!",
                ReferralCodeUsed = "REF070",
                Phone = "1233448945",
                Status = UserStatus.NonVerified,
                RegistrationDate = DateTime.UtcNow,
            };

            context.Clients.Add(client);
            context.SaveChanges();
            return client;
        }
    }
}
