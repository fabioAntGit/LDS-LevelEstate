using API.Data;
using API.Imoveis.DTOs;
using API.Models;
using API.Models.Enums;
using API.PesquisasFiltros.Filtros.Exceptions;
using API.PesquisasFiltros.Filtros.Services;
using API.PesquisasFiltros.Pesquisas.DTOs;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.PesquisasFiltros.Filtros
{
    public class PropertyOrderTest
    {
        private readonly LevelEstateContext context;
        private readonly PropertyOrderService propertyOrderService;

        public PropertyOrderTest()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                    .UseInMemoryDatabase($"OrderPropertiesDB_{Guid.NewGuid()}").Options
            );

            SeedFakeAgents();

            propertyOrderService = new PropertyOrderService(context);
        }

        [Fact]
        public void OrderProperties_ByPriceDescending_ReturnsCorrectOrder()
        {
            context.Database.EnsureDeleted();
            var properties = CreateFakeProperties();
            var result = propertyOrderService.OrderProperties(properties, isPriceDesc: false, isDateDesc: null, bestAgent: null).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(200000, result[2].Price);
            Assert.Equal(150000, result[1].Price);
            Assert.Equal(100000, result[0].Price);
        }

        [Fact]
        public void OrderProperties_ByDateDescending_ReturnsMostRecentFirst()
        {
            context.Database.EnsureDeleted();
            var properties = CreateFakeProperties();
            var result = propertyOrderService.OrderProperties(properties, isPriceDesc: null, isDateDesc: true, bestAgent: null).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal("Casa C", result.First().Title);
        }

        [Fact]
        public void OrderProperties_WhenEmptyList_Throws()
        {
            context.Database.EnsureDeleted();
            var properties = new List<PropertyListDTO>();

            Assert.Throws<PropertyOrderNotFoundException>(
                () => propertyOrderService.OrderProperties(properties, null, null, null)
            );
        }

        [Fact]
        public void OrderProperties_ByBestAgentDescending_ReturnsAgentWithMostPointsFirst()
        {
            var properties = CreateFakeProperties();
            var result = propertyOrderService.OrderProperties(properties, isPriceDesc: null, isDateDesc: null, bestAgent: true).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(2, result.First().AgentId);
        }

        private void SeedFakeAgents()
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

            context.Agents.AddRange(agents);
            context.SaveChanges();
        }

        private List<PropertyListDTO> CreateFakeProperties()
        {
            return new List<PropertyListDTO>
            {
                new PropertyListDTO
                {
                    PropertyId = 1,
                    Title = "Casa A",
                    Description = "Casa A descricao",
                    Price = 100000,
                    Status = Status.Available,
                    Topology = Topologies.House,
                    Pictures = new List<PictureDTO>
                    {
                        new PictureDTO { PictureId = 1, ImagePath = "/img/casa_a.jpg" }
                    },
                    UpdatedAt = DateTime.UtcNow.AddDays(-5),
                    AgentId = 1,
                },
                new PropertyListDTO
                {
                    PropertyId = 2,
                    Title = "Casa B",
                    Description = "Casa B descricao",
                    Price = 200000,
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    AgentId = 2,
                    Status = Status.Available,
                    Topology = Topologies.Apartment,
                    Pictures = new List<PictureDTO>
                    {
                        new PictureDTO { PictureId = 1, ImagePath = "/img/casa_b.jpg" }
                    },
                },
                new PropertyListDTO
                {
                    PropertyId = 3,
                    Title = "Casa C",
                    Description = "Casa C descricao",
                    Price = 150000,
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    AgentId = 3,
                    Status = Status.Available,
                    Topology = Topologies.Apartment,
                    Pictures = new List<PictureDTO>
                    {
                        new PictureDTO { PictureId = 1, ImagePath = "/img/casa_c.jpg" }
                    },
                },
                new PropertyListDTO
                {
                    PropertyId = 4,
                    Title = "Casa Inativa",
                    Description = "Casa Inativa descricao",
                    Price = 300000,
                    UpdatedAt = DateTime.UtcNow,
                    AgentId = 1,
                    Status = Status.Deleted,
                    Topology = Topologies.Apartment,
                    Pictures = new List<PictureDTO>
                    {
                        new PictureDTO { PictureId = 1, ImagePath = "/img/casa_inativa.jpg" }
                    }
                }
            };
        }
    }
}
