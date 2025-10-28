using API.Data;
using API.Models;
using API.Models.Enums;
using API.PesquisasFiltros.Pesquisas.Interfaces;
using API.PesquisasFiltros.Pesquisas.Services;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.PesquisasFiltros.Pesquisas
{
    public class PropertySearchTest
    {
        private readonly LevelEstateContext context;
        private readonly IPropertySearchService propertySearchService;

        public PropertySearchTest()
        {
            context = new LevelEstateContext(new DbContextOptionsBuilder<LevelEstateContext>()
                    .UseInMemoryDatabase(databaseName: $"PropertySearchDB_{Guid.NewGuid()}").Options
            );

            propertySearchService = new PropertySearchService(context);
        }

        [Fact]
        public void GetPropertiesByFilter_NoFilters_ReturnsAllProperties()
        {
            context.Database.EnsureDeleted();
            
            var properties = new List<Property>
            {
                CreateFakeProperty(
                    id: 1,
                    title: "Apartamento T2",
                    price: 200000,
                    topology: Topologies.Apartment,
                    operation: TypeOperations.Sale,
                    lat: (decimal)41.1579,
                    lon: (decimal)-8.6291
                ),
                CreateFakeProperty(
                    id: 2,
                    title: "Moradia T3",
                    price: 350000,
                    topology: Topologies.House,
                    operation: TypeOperations.Sale,
                    lat: (decimal)41.1496,
                    lon: (decimal)-8.6109
                )
            };

            context.Properties.AddRange(properties);
            context.SaveChanges();

            
            var result = propertySearchService.GetPropertiesByFilter(
                topology: null,
                minPrice: null,
                maxPrice: null,
                typeOperation: null,
                latitude: null,
                longitude: null
            ).ToList();

           
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetPropertiesByFilter_TopologyFilter_ReturnsOnlyMatching()
        {
            context.Database.EnsureDeleted();
            
            var p1 = CreateFakeProperty(
                id: 1,
                title: "T2",
                price: 150000,
                topology: Topologies.Apartment,
                operation: TypeOperations.Sale,
                lat: (decimal)41.1579,
                lon: (decimal)-8.6291
            );

            var p2 = CreateFakeProperty(
                id: 2,
                title: "T3",
                price: 250000,
                topology: Topologies.House,
                operation: TypeOperations.Sale,
                lat: (decimal)41.1496,
                lon: (decimal)-8.6109
            );

            context.Properties.AddRange(p1, p2);
            context.SaveChanges();

            
            var result = propertySearchService.GetPropertiesByFilter(
                topology: Topologies.Apartment,
                minPrice: null,
                maxPrice: null,
                typeOperation: null,
                latitude: null,
                longitude: null
            ).ToList();

            
            Assert.Equal(1, result.Count);
        }

        [Fact]
        public void GetPropertiesByFilter_TypeOperationFilter_ReturnsOnlyMatching()
        {
            context.Database.EnsureDeleted();

            var p1 = CreateFakeProperty(
                id: 1,
                title: "T2",
                price: 150000,
                topology: Topologies.Apartment,
                operation: TypeOperations.Sale,
                lat: (decimal)41.1579,
                lon: (decimal)-8.6291
            );

            var p2 = CreateFakeProperty(
                id: 2,
                title: "T3",
                price: 250000,
                topology: Topologies.House,
                operation: TypeOperations.Lease,
                lat: (decimal)41.1496,
                lon: (decimal)-8.6109
            );

            context.Properties.AddRange(p1, p2);
            context.SaveChanges();


            var result = propertySearchService.GetPropertiesByFilter(
                topology: null,
                minPrice: null,
                maxPrice: null,
                typeOperation: TypeOperations.Lease,
                latitude: null,
                longitude: null
            ).ToList();


            Assert.Equal(1, result.Count);
        }

        [Fact]
        public void GetPropertiesByFilter_LatLonFilter_ReturnsOnlyMatching()
        {
            context.Database.EnsureDeleted();

            var p1 = CreateFakeProperty(
                id: 1,
                title: "T2",
                price: 150000,
                topology: Topologies.Apartment,
                operation: TypeOperations.Sale,
                lat: (decimal)-50.822,
                lon: (decimal)-8.6291
            );

            var p2 = CreateFakeProperty(
                id: 2,
                title: "T3",
                price: 250000,
                topology: Topologies.House,
                operation: TypeOperations.Lease,
                lat: (decimal)41.1496,
                lon: (decimal)-8.6109
            );

            context.Properties.AddRange(p1, p2);
            context.SaveChanges();


            var result = propertySearchService.GetPropertiesByFilter(
                topology: null,
                minPrice: null,
                maxPrice: null,
                typeOperation: null,
                latitude: 41.1579,
                longitude: -8.6291
            ).ToList();


            Assert.Equal(1, result.Count);
        }

        [Fact]
        public void GetPropertiesByFilter_MinMaxFilter_ReturnsOnlyMatching()
        {
            context.Database.EnsureDeleted();

            var p1 = CreateFakeProperty(
                id: 1,
                title: "T2",
                price: 150000,
                topology: Topologies.Apartment,
                operation: TypeOperations.Sale,
                lat: (decimal)41.1579,
                lon: (decimal)-8.6291
            );

            var p2 = CreateFakeProperty(
                id: 2,
                title: "T3",
                price: 250000,
                topology: Topologies.House,
                operation: TypeOperations.Lease,
                lat: (decimal)41.1496,
                lon: (decimal)-8.6109
            );

            context.Properties.AddRange(p1, p2);
            context.SaveChanges();


            var result = propertySearchService.GetPropertiesByFilter(
                topology: null,
                minPrice: 200000,
                maxPrice: 250000,
                typeOperation: null,
                latitude: null,
                longitude: null
            ).ToList();


            Assert.Equal(1, result.Count);
        }

        [Fact]
        public void GetPropertiesByFilter_ExcludesDeletedArchivedUnavailable_ReturnsOnlyMatching()
        {
            context.Database.EnsureDeleted();
            
            var available = CreateFakeProperty(1, "Disponível", 200000, Topologies.Apartment, TypeOperations.Sale, 41.15m, -8.61m, Status.Available);
            var deleted = CreateFakeProperty(2, "Apagado", 150000, Topologies.Apartment, TypeOperations.Sale, 41.16m, -8.63m, Status.Deleted);
            var archived = CreateFakeProperty(3, "Arquivado", 180000, Topologies.Apartment, TypeOperations.Sale, 41.17m, -8.62m, Status.Archived);
            var unavailable = CreateFakeProperty(4, "Indisponível", 300000, Topologies.Apartment, TypeOperations.Sale, 41.18m, -8.60m, Status.Unavailable);

            context.Properties.AddRange(available, deleted, archived, unavailable);
            context.SaveChanges();

             
            var result = propertySearchService.GetPropertiesByFilter(Topologies.Apartment, null, null, null, null, null).ToList();
                
            Assert.Equal(1, result.Count);
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
