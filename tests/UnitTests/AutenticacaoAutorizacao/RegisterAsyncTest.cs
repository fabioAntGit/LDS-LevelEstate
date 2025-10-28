using API.AutenticacaoAutorizacao.DTOs;
using API.AutenticacaoAutorizacao.Exceptions;
using API.AutenticacaoAutorizacao.Services;
using API.Data;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Interfaces;
using API.Perfis.Agente.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UnitTests.AutenticacaoAutorizacao
{
    public class RegisterAsyncTest
    {
        private readonly LevelEstateContext context;
        private readonly AuthService authService;
        private readonly IAgentService agentService;
        private readonly IConfiguration config;

        public RegisterAsyncTest()
        {
            context = new LevelEstateContext(
                new DbContextOptionsBuilder<LevelEstateContext>()
                    .UseInMemoryDatabase($"RegisterAsyncTestDB_{Guid.NewGuid()}")
                    .Options
            );

            Env.TraversePath().Load();
            config = new ConfigurationBuilder()
            .AddEnvironmentVariables("")
            .Build();

            agentService = new AgentService(context);
            authService = new AuthService(context, agentService, config);
        }

        [Fact]
        public async Task Register_ValidAgent_ReturnsTrueAndSavesToDatabase()
        {
            var registerDTO = new RegisterDTO
            {
                FirstName = "João",
                LastName = "Silva",
                Phone = "910000000",
                Email = "joao@teste.com",
                Password = "Strong@123",
                ConfirmedPassword = "Strong@123",
                Role = Role.Agent,
                AMICode = "AMI100"
            };

            var result = await authService.RegisterAsync(registerDTO);

            Assert.True(result);
            Assert.Single(context.Agents);

            var agent = await context.Agents.FirstAsync();

            Assert.Equal("joao@teste.com", agent.Email);
            Assert.True(!string.IsNullOrEmpty(agent.ReferralCode));
        }

        [Fact]
        public async Task Register_ValidClient_ReturnsTrueAndSavesToDatabase()
        {
            var registerDTO = new RegisterDTO
            {
                FirstName = "Maria",
                LastName = "Oliveira",
                Phone = "910000001",
                Email = "maria@teste.com",
                Password = "Strong@123",
                ConfirmedPassword = "Strong@123",
                Role = Role.Client
            };

            var result = await authService.RegisterAsync(registerDTO);

            Assert.True(result);
            Assert.Single(context.Clients);

            var client = await context.Clients.FirstAsync();

            Assert.Equal("maria@teste.com", client.Email);
        }

        [Fact]
        public async Task Register_MissingFields_ThrowsInvalidInputException()
        {
            var dto = new RegisterDTO
            {
                FirstName = "",
                LastName = "",
                Email = "",
                Password = "",
                ConfirmedPassword = ""
            };

            await Assert.ThrowsAsync<InvalidInputRegisterDTOException>(async () => await authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task Register_WeakPassword_ThrowsInvalidInputException()
        {
            var dto = new RegisterDTO
            {
                FirstName = "Ana",
                LastName = "Costa",
                Phone = "910000002",
                Email = "ana@teste.com",
                Password = "12345",
                ConfirmedPassword = "12345",
                Role = Role.Client
            };

            await Assert.ThrowsAsync<InvalidInputRegisterDTOException>(async  () => await authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task Register_PasswordsDoNotMatch_ThrowsInvalidInputException()
        {
            var dto = new RegisterDTO
            {
                FirstName = "Pedro",
                LastName = "Mendes",
                Phone = "910000003",
                Email = "pedro@teste.com",
                Password = "Strong@123",
                ConfirmedPassword = "Different@123",
                Role = Role.Client
            };

            await Assert.ThrowsAsync<InvalidInputRegisterDTOException>(async () => await authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsInvalidInputException()
        {
            context.Users.Add(new Agent
            {
                FirstName = "João",
                LastName = "Silva",
                Phone = "910000004",
                Email = "joao@teste.com",
                Password = "hash",
                Role = Role.Agent,
                AMICode = "AMI050",
                ReferralCode = "REF050",
                Status = UserStatus.Verified
            });
            context.SaveChanges();

            var dto = new RegisterDTO
            {
                FirstName = "Outro",
                LastName = "Teste",
                Phone = "910000005",
                Email = "joao@teste.com",
                Password = "Strong@123",
                ConfirmedPassword = "Strong@123",
                Role = Role.Client
            };

            await Assert.ThrowsAsync<InvalidInputRegisterDTOException>(async () => await authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task Register_AgentWithoutAMICode_ThrowsInvalidInputException()
        {
            var dto = new RegisterDTO
            {
                FirstName = "Carlos",
                LastName = "Souza",
                Phone = "910000006",
                Email = "carlos@teste.com",
                Password = "Strong@123",
                ConfirmedPassword = "Strong@123",
                Role = Role.Agent,
                AMICode = ""
            };

            await Assert.ThrowsAsync<InvalidInputRegisterDTOException>(async () => await authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task Register_AgentDuplicateAMICode_ThrowsInvalidInputException()
        {
            context.Agents.Add(new Agent
            {
                FirstName = "Miguel",
                LastName = "Ferreira",
                Phone = "910000007",
                Email = "miguel@teste.com",
                Password = "hash",
                Role = Role.Agent,
                AMICode = "AMI001",
                ReferralCode = "REF001",
                Status = UserStatus.Verified
            });
            context.SaveChanges();

            var dto = new RegisterDTO
            {
                FirstName = "Rita",
                LastName = "Pereira",
                Phone = "910000008",
                Email = "rita@teste.com",
                Password = "Strong@123",
                ConfirmedPassword = "Strong@123",
                Role = Role.Agent,
                AMICode = "AMI001"
            };

            await Assert.ThrowsAsync<InvalidInputRegisterDTOException>(async () => await authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task Register_DefaultProfileImageIsSet_WhenPfpImageIsNull()
        {
            var dto = new RegisterDTO
            {
                FirstName = "Tiago",
                LastName = "Lopes",
                Phone = "910000009",
                Email = "tiago@teste.com",
                Password = "Strong@123",
                ConfirmedPassword = "Strong@123",
                Role = Role.Client,
                PfpImage = null
            };

            var result = await authService.RegisterAsync(dto);

            Assert.True(result);
            var client = await context.Clients.FirstAsync();
            Assert.Equal("/../Perfis/Images/defaultPfp.jpg", client.PfpImage);
        }
    }
}
