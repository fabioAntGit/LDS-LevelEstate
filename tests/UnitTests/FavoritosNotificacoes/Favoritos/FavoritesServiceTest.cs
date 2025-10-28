using API.Data;
using API.FavoritosNotificacoes.Favoritos.Services;
using API.Imoveis.DTOs;
using API.Models;
using API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.FavoritosNotificacoes.Favoritos
{
    public class FavoritesServiceTest
    {
        private readonly LevelEstateContext context;
        private readonly FavoriteService service;

        public FavoritesServiceTest()
        {
            var options = new DbContextOptionsBuilder<LevelEstateContext>()
                .UseInMemoryDatabase($"FavoritesTestDB_{Guid.NewGuid()}")
                .Options;

            context = new LevelEstateContext(options);
            service = new FavoriteService(context);

            SeedData();
        }

        [Fact]
        public void AddFavorite_ValidProperty_AddsSuccessfully()
        {
            // Act
            var result = service.AddFavorite(1, 1);

            // Assert
            Assert.True(result);

            var client = context.Clients.
                Include(c => c.Favorite).First(c => c.UserId == 1);
            Assert.Single(client.Favorite);
            Assert.Equal(1, client.Favorite.First().PropertyId);
        }

        [Fact]
        public void AddFavorite_PropertyAlreadyExists_ThrowsInvalidOperation()
        {
            // Arrange
            service.AddFavorite(1, 1);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.AddFavorite(1, 1));
        }

        [Fact]
        public void AddFavorite_InvalidProperty_ThrowsPropertyNotFound()
        {
            Assert.Throws<API.Imoveis.Exceptions.PropertyNotFoundException>(
                () => service.AddFavorite(999, 1));
        }

        [Fact]
        public void RemoveFavorite_ExistingProperty_RemovesSuccessfully()
        {
            // Arrange
            service.AddFavorite(1, 1);

            // Act
            var result = service.RemoveFavorite(1, 1);

            // Assert
            Assert.True(result);

            var client = context.Clients.Include(c => c.Favorite).First(c => c.UserId == 1);
            Assert.Empty(client.Favorite);
        }

        [Fact]
        public void RemoveFavorite_NonExistingProperty_ThrowsPropertyNotFound()
        {

            Assert.Throws<API.Imoveis.Exceptions.PropertyNotFoundException>(
                () => service.RemoveFavorite(999, 1));
        }

        [Fact]
        public void ListFavorites_ReturnsFavoritesForClient()
        { 
           // Arrange
           service.AddFavorite(1, 1); 
           service.AddFavorite(2, 1); 

           // Act
           var favorites = service.ListFavorites(1); 

           // Assert
           Assert.Equal(2, favorites.Count()); 
           Assert.All(favorites, f => Assert.IsType<PropertyDTO>(f)); 
        }
        private void SeedData()
        {
            var clients = new List<Client>
            {
                new Client
                {
                    UserId = 1,
                    FirstName = "João",
                    LastName = "Silva",
                    Email = "joao@email.com",
                    Password = "123",
                    Phone = "910000000",
                    Role = API.Models.Enums.Role.Client,
                    Favorite = new List<Property>()
                },
                new Client
                {
                    UserId = 2,
                    FirstName = "Maria",
                    LastName = "Santos",
                    Email = "maria@email.com",
                    Password = "456",
                    Phone = "920000000",
                    Favorite = new List<Property>()
                }
            };

            var properties = new List<Property>
            {
                CreateFakeProperty(1, "Apartamento Teste 1"),
                CreateFakeProperty(2, "Apartamento Teste 2")
            };

            context.Clients.AddRange(clients);
            context.Properties.AddRange(properties);
            context.SaveChanges();
        }
        private Property CreateFakeProperty(int id, string title)
        {
            return new Property
            {
                PropertyId = id,
                AgentId = 1,
                Street = "Rua Teste",
                PostalCode = "4000-000",
                Title = title,
                Status = Status.Available,
                Price = 120000,
                Topology = Topologies.Apartment,
                TypeOperation = TypeOperations.Sale,
                Latitude = 41.15m,
                Longitude = -8.61m,
                Description = "Teste",
                Area = 100,
                Floor = 1,
                NumberOfFloors = 4,
                Bedrooms = 2,
                Bathrooms = 1,
                HasGarage = true,
                HasElevator = true,
                BuiltAt = 2015,
                CreatedAt = DateTime.UtcNow,
                Pictures = new List<Picture>
                {
                    new Picture { PictureId = id, ImagePath = $"/img/{title.ToLower().Replace(" ", "_")}.jpg" }
                }
            };
        }
    }
}
