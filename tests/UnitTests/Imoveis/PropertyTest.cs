using API.Data;
using API.FavoritosNotificacoes.Notificacoes.Services;
using API.Imoveis.DTOs;
using API.Imoveis.Exceptions;
using API.Imoveis.Services;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Exceptions;
using API.Perfis.Agente.Services;
using Microsoft.EntityFrameworkCore;
using UnitTests.Common;

namespace UnitTests.Imoveis
{
    public class PropertyTest
    {
        private readonly LevelEstateContext context;
        private readonly PropertyService propertyService;
        private readonly AgentService agentService;

        public PropertyTest()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                .UseInMemoryDatabase($"PropertyTestsDB_{Guid.NewGuid()}").Options
            );

            context.Database.EnsureDeleted();

            SeedFakeAgents();

            var fakeHandler = new FakeNotificationHandler();
            var fakeNotificationService = new NotificationService(context, fakeHandler);

            agentService = new AgentService(context);

            propertyService = new PropertyService(context, fakeNotificationService, agentService);
        }

        [Fact]
        public void CreateProperty_WhenValidData_CreatesSuccessfully()
        {
            // Arrange
            var agent = context.Agents.First(a => a.UserId == 1);

            var dto = new CreatePropertyDTO
            {
                Title = "Apartamento Novo",
                Street = "Rua Nova 123",
                PostalCode = "4000-001",
                Price = 150000,
                Topology = Topologies.Apartment,
                TypeOperation = TypeOperations.Sale,
                Area = 120,
                Floor = 2,
                NumberOfFloors = 5,
                Bedrooms = 3,
                Bathrooms = 2,
                HasGarage = true,
                HasElevator = true,
                BuiltAt = 2020,
                AirConditioning = true,
                Furnished = false,
                Balcony = true,
                Terrace = false,
                Latitude = 41.15m,
                Longitude = -8.61m,
                Description = "Apartamento moderno com boa localização"
            };

            // Act
            var result = propertyService.CreateProperty(dto, agent.UserId);

            Assert.NotNull(result);
            Assert.Equal(dto.Title, result.Title);
            Assert.Equal(dto.Price, result.Price);
            Assert.Equal(Status.Available, result.Status);
            Assert.Equal(agent.UserId, result.AgentId);

            var created = context.Properties.FirstOrDefault(p => p.Title == dto.Title);
            Assert.NotNull(created);
            Assert.Equal(dto.Price, created.Price);
            Assert.Equal(Status.Available, created.Status);
            Assert.Equal(agent.UserId, created.AgentId);
        }

        [Fact]
        public void CreateProperty_WhenMissingRequiredFields_ThrowsInvalidPropertyDataException()
        {
            var agent = context.Agents.First(a => a.UserId == 1);

            var dto = new CreatePropertyDTO
            {
                Title = "",
                Street = "Rua Nova 123",
                PostalCode = "4000-001",
                Price = 150000,
                Latitude = 0,
                Longitude = -8.61m
            };

            Assert.Throws<InvalidPropertyDataException>(() =>
                propertyService.CreateProperty(dto, agent.UserId)
            );
        }

        [Fact]
        public void CreateProperty_WhenPostalCodeInvalid_ThrowsInvalidPropertyDataException()
        {
            var agent = context.Agents.First(a => a.UserId == 1);

            var dto = new CreatePropertyDTO
            {
                Title = "Apartamento",
                Street = "Rua Nova",
                PostalCode = "4000001",
                Price = 100000,
                Topology = Topologies.Apartment,
                TypeOperation = TypeOperations.Sale,
                Area = 100,
                Latitude = 41.15m,
                Longitude = -8.61m,
                BuiltAt = 2010
            };

            Assert.Throws<InvalidPropertyDataException>(() =>
                propertyService.CreateProperty(dto, agent.UserId)
            );
        }

        [Fact]
        public async Task UpdateProperty_WhenExists_UpdatesSuccessfully()
        {
            var agent = context.Agents.First(a => a.UserId == 1);

            var property = CreateFakeProperty(agent.UserId, "Antigo", 100000, Topologies.Apartment, TypeOperations.Sale, 41.15m, -8.61m);
            context.Properties.Add(property);
            context.SaveChanges();

            var dto = new UpdatePropertyDTO
            {
                PropertyId = property.PropertyId,
                Title = "Atualizado",
                Street = property.Street,
                PostalCode = property.PostalCode,
                Price = 200000,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                Description = "Descrição atualizada",
                Status = Status.Available,
                Topology = property.Topology,
                TypeOperation = property.TypeOperation,
                Area = property.Area,
                Floor = property.Floor,
                NumberOfFloors = property.NumberOfFloors,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                HasGarage = property.HasGarage,
                HasElevator = property.HasElevator,
                BuiltAt = property.BuiltAt,
                AirConditioning = property.AirConditioning,
                Furnished = property.Furnished,
                Balcony = property.Balcony,
                Terrace = property.Terrace
            };

            var result = await propertyService.UpdatePropertyAsync(dto, agent.UserId);

            Assert.NotNull(result);
            Assert.Equal("Atualizado", result.Title);
            Assert.Equal(200000, result.Price);
            Assert.Equal(property.PropertyId, result.PropertyId);

            var updated = context.Properties.First(p => p.PropertyId == property.PropertyId);
            Assert.Equal("Atualizado", updated.Title);
            Assert.Equal(200000, updated.Price);
            Assert.Equal("Descrição atualizada", updated.Description);
        }

        [Fact]
        public async Task UpdateProperty_WhenPropertyNotFound_ThrowsPropertyNotFoundException()
        {
            var agent = context.Agents.First(a => a.UserId == 1);

            var dto = new UpdatePropertyDTO
            {
                PropertyId = 999,
                Title = "Inexistente",
                Street = "Rua Fantasma",
                PostalCode = "0000-000",
                Price = 100000,
                Latitude = 41.15m,
                Longitude = -8.61m,
                Description = "Imóvel que não existe",
                Status = Status.Available,
                Topology = Topologies.Apartment,
                TypeOperation = TypeOperations.Sale,
                Area = 100,
                Floor = 1,
                NumberOfFloors = 3,
                Bedrooms = 2,
                Bathrooms = 1,
                HasGarage = false,
                HasElevator = false,
                BuiltAt = 2010,
                AirConditioning = false,
                Furnished = false,
                Balcony = false,
                Terrace = false
            };

            await Assert.ThrowsAsync<PropertyNotFoundException>(() =>
                propertyService.UpdatePropertyAsync(dto, agent.UserId)
            );
        }

        [Fact]
        public async Task UpdateProperty_WhenMissingRequiredFields_ThrowsInvalidPropertyDataException()
        {
            var agent = context.Agents.First(a => a.UserId == 1);

            var property = CreateFakeProperty(agent.UserId, "Casa Teste", 150000, Topologies.Apartment, TypeOperations.Sale, 41.15m, -8.61m);
            context.Properties.Add(property);
            context.SaveChanges();

            var dto = new UpdatePropertyDTO
            {
                PropertyId = property.PropertyId,
                Title = "",
                Street = "",
                PostalCode = "",
                Latitude = 0,
                Longitude = 0,
                Price = 150000
            };

            await Assert.ThrowsAsync<InvalidPropertyDataException>(() =>
                propertyService.UpdatePropertyAsync(dto, agent.UserId)
            );
        }

        [Fact]
        public async Task UpdateProperty_WhenAgentNotOwner_ThrowsAgentNotAuthorizedException()
        {
            var owner = context.Agents.First(a => a.UserId == 1);
            var otherAgent = context.Agents.First(a => a.UserId == 2);

            var property = CreateFakeProperty(owner.UserId, "Casa do Dono", 150000, Topologies.Apartment, TypeOperations.Sale, 41.15m, -8.61m);
            context.Properties.Add(property);
            context.SaveChanges();

            var dto = new UpdatePropertyDTO
            {
                PropertyId = property.PropertyId,
                Title = "Tentativa de Atualização Indevida",
                Street = property.Street,
                PostalCode = property.PostalCode,
                Price = property.Price,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                Description = property.Description,
                Status = property.Status,
                Topology = property.Topology,
                TypeOperation = property.TypeOperation,
                Area = property.Area,
                Floor = property.Floor,
                NumberOfFloors = property.NumberOfFloors,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                HasGarage = property.HasGarage,
                HasElevator = property.HasElevator,
                BuiltAt = property.BuiltAt,
                AirConditioning = property.AirConditioning,
                Furnished = property.Furnished,
                Balcony = property.Balcony,
                Terrace = property.Terrace
            };

            await Assert.ThrowsAsync<AgentNotAuthorizedException>(() =>
                propertyService.UpdatePropertyAsync(dto, otherAgent.UserId)
            );
        }

        [Fact]
        public async Task UpdateProperty_WhenStatusChangesToSold_AddsPointsToAgent()
        {
            // Arrange
            var agent = context.Agents.First(a => a.UserId == 1);
            var initialPoints = agent.MonthlyPoints;

            var property = CreateFakeProperty(agent.UserId, "Venda Teste", 250000, Topologies.Apartment, TypeOperations.Sale, 41.15m, -8.61m);
            context.Properties.Add(property);
            context.SaveChanges();

            var dto = new UpdatePropertyDTO
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                Street = property.Street,
                PostalCode = property.PostalCode,
                Price = 250000,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                Description = "Vendida",
                Status = Status.Sold,
                Topology = property.Topology,
                TypeOperation = property.TypeOperation,
                Area = property.Area,
                Floor = property.Floor,
                NumberOfFloors = property.NumberOfFloors,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                HasGarage = property.HasGarage,
                HasElevator = property.HasElevator,
                BuiltAt = property.BuiltAt,
                AirConditioning = property.AirConditioning,
                Furnished = property.Furnished,
                Balcony = property.Balcony,
                Terrace = property.Terrace
            };

            // Act
            var result = await propertyService.UpdatePropertyAsync(dto, agent.UserId);

            // Assert
            var updatedAgent = context.Agents.First(a => a.UserId == agent.UserId);
            Assert.Equal(initialPoints + 100, updatedAgent.MonthlyPoints); 
        }

        [Fact]
        public async Task UpdateProperty_WhenStatusUnchanged_DoesNotAddPoints()
        {
            // Arrange
            var agent = context.Agents.First(a => a.UserId == 1);

            var property = CreateFakeProperty(agent.UserId, "Venda Teste 2", 200000, Topologies.Apartment, TypeOperations.Sale, 41.15m, -8.61m, Status.Sold);
            context.Properties.Add(property);
            context.SaveChanges();

            var initialPoints = agent.MonthlyPoints;

            var dto = new UpdatePropertyDTO
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                Street = property.Street,
                PostalCode = property.PostalCode,
                Price = 200000,
                Latitude = property.Latitude,
                Longitude = property.Longitude,
                Description = "Sem mudança",
                Status = Status.Sold,
                Topology = property.Topology,
                TypeOperation = property.TypeOperation,
                Area = property.Area,
                Floor = property.Floor,
                NumberOfFloors = property.NumberOfFloors,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                HasGarage = property.HasGarage,
                HasElevator = property.HasElevator,
                BuiltAt = property.BuiltAt,
                AirConditioning = property.AirConditioning,
                Furnished = property.Furnished,
                Balcony = property.Balcony,
                Terrace = property.Terrace
            };

            // Act
            await propertyService.UpdatePropertyAsync(dto, agent.UserId);

            // Assert
            var updatedAgent = context.Agents.First(a => a.UserId == agent.UserId);
            Assert.Equal(initialPoints, updatedAgent.MonthlyPoints);
        }

        [Fact]
        public void DeleteProperty_WhenAgentOwnsProperty_DeletesSuccessfully()
        {
            var agent = context.Users.First(u => u.UserId == 1);

            var property = CreateFakeProperty(agent.UserId, "Apagar", 150000, Topologies.House, TypeOperations.Sale, 41.12m, -8.62m);
            context.Properties.Add(property);
            context.SaveChanges();

            var result = propertyService.DeleteProperty(property.PropertyId, agent.UserId);

            Assert.True(result);
            var deleted = context.Properties.First(p => p.PropertyId == property.PropertyId);
            Assert.Equal(Status.Deleted, deleted.Status);
        }

        [Fact]
        public void DeleteProperty_WhenPropertyAlreadyDeleted_ThrowsInvalidOperationException()
        {
            var agent = context.Users.First(u => u.UserId == 1);

            var property = CreateFakeProperty(agent.UserId, "Apagado", 150000, Topologies.House, TypeOperations.Sale, 41.12m, -8.62m, Status.Deleted);
            context.Properties.Add(property);
            context.SaveChanges();

            Assert.Throws<InvalidOperationException>(() =>
                propertyService.DeleteProperty(property.PropertyId, agent.UserId)
            );
        }

        [Fact]
        public void DeleteProperty_WhenPropertyNotFound_ThrowsPropertyNotFoundException()
        {
            var agent = context.Users.First(u => u.UserId == 3);

            Assert.Throws<PropertyNotFoundException>(() =>
                propertyService.DeleteProperty(999, agent.UserId)
            );
        }


        [Fact]
        public void DeleteProperty_WhenAgentNotOwner_ThrowsAgentNotAuthorizedException()
        {
            var owner = context.Users.First(u => u.UserId == 1);
            var otherAgent = context.Users.First(u => u.UserId == 2);

            var property = CreateFakeProperty(owner.UserId, "OutroAgente", 150000, Topologies.Apartment, TypeOperations.Sale, 41.15m, -8.61m);
            context.Properties.Add(property);
            context.SaveChanges();

            Assert.Throws<AgentNotAuthorizedException>(() =>
                propertyService.DeleteProperty(property.PropertyId, otherAgent.UserId)
            );
        }

        [Fact]
        public void GetPropertiesOf_WhenAgentExists_ReturnsAgentProperties()
        {
            var available = CreateFakeProperty(1, "Disponível", 200000, Topologies.Apartment, TypeOperations.Sale, 41.15m, -8.61m, Status.Available);
            var deleted = CreateFakeProperty(2, "Apagado", 150000, Topologies.Apartment, TypeOperations.Sale, 41.16m, -8.63m, Status.Deleted);
            context.Properties.Add(available);
            context.Properties.Add(deleted);
            context.SaveChanges();

            var result = propertyService.GetPropertiesOf(1).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(1, p.AgentId));
        }

        [Fact]
        public void GetPropertiesOf_WhenAgentHasNoProperties_ThrowsPropertyNotFoundException()
        {
            context.Users.Add(new Agent
            {
                UserId = 4,
                FirstName = "SemPropriedades",
                LastName = "Agente",
                Email = "sem@teste.com",
                Password = "123",
                Role = Role.Agent,
                AMICode = "AMI009",
                ReferralCode = "REF009",
                MonthlyPoints = 0,
                AnnualPoints = 0,
                Phone = "910000004",
                AllMoneyEarned = 0,
                MoneyEarnedLastMonth = 0,
                RegistrationDate = DateTime.UtcNow,
                Status = UserStatus.Verified
            });
            context.SaveChanges();

            Assert.Throws<PropertyNotFoundException>(() => propertyService.GetPropertiesOf(3));
        }

        [Fact]
        public void GetPropertyDetails_WhenExists_ReturnsProperty()
        {
            var available = CreateFakeProperty(1, "Disponível", 200000, Topologies.Apartment, TypeOperations.Sale, 41.15m, -8.61m, Status.Available);
            context.Properties.Add(available);
            context.SaveChanges();

            var result = propertyService.GetPropertyDetails(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.PropertyId);
            Assert.Equal("Disponível", result.Title);
            Assert.Equal("Ana", result.AgentFirstName);
            Assert.Equal("Silva", result.AgentLastName);
        }

        [Fact]
        public void GetPropertyDetails_WhenPropertyNotFound_ThrowsPropertyNotFoundException()
        {
            Assert.Throws<PropertyNotFoundException>(() =>
                propertyService.GetPropertyDetails(999)
            );
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

        private Property CreateFakeProperty(
            int id,
            string title,
            decimal price,
            Topologies topology,
            TypeOperations operation,
            decimal lat,
            decimal lon,
            Status status = Status.Available)
        {
            return new Property
            {
                PropertyId = id,
                AgentId = 1,
                Street = "Rua Exemplo 123",
                PostalCode = "4000-123",
                Latitude = lat,
                Longitude = lon,
                Title = title,
                Description = "Descrição de teste",
                Price = price,
                Status = status,
                Topology = topology,
                TypeOperation = operation,
                Pictures = new List<Picture>
                {
                    new Picture { PictureId = id, ImagePath = $"/img/{title.ToLower().Replace(" ", "_")}.jpg" }
                },
                Area = 120.0m,
                Floor = 2,
                NumberOfFloors = 4,
                Bedrooms = 3,
                Bathrooms = 2,
                HasGarage = true,
                HasElevator = true,
                BuiltAt = 2005,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
