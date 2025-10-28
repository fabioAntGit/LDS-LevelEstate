using API.AutenticacaoAutorizacao.Exceptions;
using API.AutenticacaoAutorizacao.Services;
using API.Data;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Interfaces;
using API.Perfis.Agente.Services;
using API.Perfis.Cliente.Exceptions;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UnitTests.AutenticacaoAutorizacao
{
    public class VerifyAccountAsyncTest
    {
        private readonly LevelEstateContext context;
        private readonly AuthService authService;
        private readonly IAgentService agentService;
        private readonly Client client;
        private readonly IConfiguration config;

        public VerifyAccountAsyncTest()
        {
            context = new LevelEstateContext(
                new DbContextOptionsBuilder<LevelEstateContext>()
                    .UseInMemoryDatabase($"VerifyAccountAsyncTestDB_{Guid.NewGuid()}")
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
        public async Task VerifyAccount_ValidToken_UpdatesUserStatusToVerified()
        {
            var token = GenerateToken(client.Email, 30, true);

            await authService.VerifyAccountAsync(token);

            Assert.Equal(UserStatus.Verified, client.Status);
        }

        [Fact]
        public async Task VerifyAccount_UserNotFound_ThrowsUserNotFoundException()
        {
            var token = GenerateToken("notFoundEmail@teste.com", 30,true);

            await Assert.ThrowsAsync<UserNotFoundException>(async () => await authService.VerifyAccountAsync(token));
        }

        [Fact]
        public async Task VerifyAccount_TokenWithoutEmailClaim_ThrowsSecurityTokenException()
        {
            var token = GenerateToken(client.Email, 30, false);

            await Assert.ThrowsAsync<SecurityTokenException>(async () => await authService.VerifyAccountAsync(token));
        }

        [Fact]
        public async Task VerifyAccount_ExpiredToken_ThrowsSecurityTokenExpiredException()
        {
            var token = GenerateToken(client.Email, -3, true);
            await Assert.ThrowsAsync<SecurityTokenExpiredException>(async () => await authService.VerifyAccountAsync(token));
        }

        [Fact]
        public async Task VerifyAccount_AlreadyVerified_ThrowsClientAlreadyVerifiedException()
        {
            client.Status = UserStatus.Verified;
            await context.SaveChangesAsync();

            var token = GenerateToken(client.Email, 30, true);

            await Assert.ThrowsAsync<ClientAlreadyVerifiedException>(async () => await authService.VerifyAccountAsync(token));
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

        private string GenerateToken(string email, double minutes, bool includeEmailClaim = true)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JwtSettings__Key")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var exp = now.AddMinutes(minutes);

            var claims = new List<Claim>();
            if (includeEmailClaim)
                claims.Add(new Claim(ClaimTypes.Email, email));
          
            var token = new JwtSecurityToken(
                issuer: config["JwtSettings:Issuer"],
                audience: config["JwtSettings:Audience"],
                claims: claims,
                notBefore: now.AddMinutes(-5),
                expires: exp,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
