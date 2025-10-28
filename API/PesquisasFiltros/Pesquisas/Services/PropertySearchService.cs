using API.Data;
using API.Imoveis.DTOs;
using API.Models.Enums;
using API.PesquisasFiltros.Pesquisas.DTOs;
using API.PesquisasFiltros.Pesquisas.Exceptions;
using API.PesquisasFiltros.Pesquisas.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.PesquisasFiltros.Pesquisas.Services
{
    public class PropertySearchService : IPropertySearchService
    {
        private readonly LevelEstateContext context;

        public PropertySearchService(LevelEstateContext context)
        {
            this.context = context;
        }
        
        public IEnumerable<PropertyListDTO> GetPropertiesByFilter(Topologies? topology, double? minPrice, double? maxPrice, TypeOperations? typeOperation, double? latitude, double? longitude)
        {
            //Check if status is available
            var validStatus = new[] { Status.Available };
            var filtered = context.Properties.Include(p => p.Pictures)
                .Where(p => validStatus.Contains(p.Status))
                .ToList();

            if (filtered.IsNullOrEmpty())
                throw new PropertySearchNotFoundException("No properties available at the moment!");

            var query = filtered.AsQueryable();

            if (topology.HasValue)
                query = query.Where(p => p.Topology == topology.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= (decimal)minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= (decimal)maxPrice.Value);

            if (typeOperation.HasValue)
                query = query.Where(p => p.TypeOperation == typeOperation.Value);

            var properties = query.ToList();

            //Filter by location, after getting properties because EF cannot convert math fucnctions 
            if (latitude.HasValue && longitude.HasValue)
            {
                const double radiusKm = 10;
                properties = properties.Where(p => CalculateDistanceKm((double)p.Latitude, (double)p.Longitude, latitude.Value,
                    longitude.Value) <= radiusKm).ToList();
            }

            var propertiesDTO = properties.Select(p => new PropertyListDTO
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Topology = p.Topology,
                Pictures = p.Pictures.Select(p => new PictureDTO
                {
                    PictureId = p.PictureId,
                    ImagePath = p.ImagePath
                }).ToList(),
                Status = p.Status,
                AgentId = p.AgentId,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            if (propertiesDTO.IsNullOrEmpty())
            {
                throw new PropertySearchNotFoundException("No properties for this filters!");
            }

            return propertiesDTO;
        }

        private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0;

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Pow(Math.Sin(dLon / 2), 2);

            double c = 2 * Math.Asin(Math.Sqrt(a));

            return EarthRadiusKm * c;
        }

        private static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
