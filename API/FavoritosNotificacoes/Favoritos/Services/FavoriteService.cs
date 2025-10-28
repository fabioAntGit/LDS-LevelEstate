using API.Data;
using API.FavoritosNotificacoes.Favoritos.Interfaces;
using API.Imoveis.DTOs;
using API.Imoveis.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace API.FavoritosNotificacoes.Favoritos.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly LevelEstateContext context;

        public FavoriteService(LevelEstateContext context)
        {
            this.context = context;
        }

        public bool AddFavorite(int propertyId, int idClient)
        {
            var property = context.Properties.FirstOrDefault(p => p.PropertyId == propertyId);

            if (property == null)
                throw new PropertyNotFoundException($"Property with ID {propertyId} not found.");

            var client = context.Clients
                .Include(c => c.Favorite)
                .FirstOrDefault(c => c.UserId == idClient);

            if (client.Favorite.Any(p => p.PropertyId == propertyId))
                throw new InvalidOperationException($"Property {propertyId} already favorited.");

            client.Favorite.Add(property);
            context.SaveChanges();
            return true;
        }

        public bool RemoveFavorite(int propertyId, int idClient)
        {
            var favorite = context.Clients
                .Where(c => c.UserId == idClient)
                .SelectMany(c => c.Favorite)
                .FirstOrDefault(p => p.PropertyId == propertyId);

            if (favorite == null)
                throw new PropertyNotFoundException($"Property {propertyId} not found in favorites.");

            var client = context.Clients.Include(c => c.Favorite)
                .First(c => c.UserId == idClient);

            client.Favorite.Remove(favorite);
            context.SaveChanges();
            return true;
        }

        public IEnumerable<PropertyDTO> ListFavorites(int idClient)
        {
            var favorites = context.Clients
                .Where(c => c.UserId == idClient && c.Role == Models.Enums.Role.Client)
                .SelectMany(c => c.Favorite)
                .Select(p => new PropertyDTO
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    Price = p.Price,
                    Topology = p.Topology,
                    Street = p.Street,
                    AgentId = p.AgentId,
                    Status = p.Status,
                    Pictures = p.Pictures.Select(pic => new PictureDTO
                    {
                        PictureId = pic.PictureId,
                        ImagePath = pic.ImagePath
                    }).ToList()
                })
                .ToList();

            return favorites;
        }
    }
}
