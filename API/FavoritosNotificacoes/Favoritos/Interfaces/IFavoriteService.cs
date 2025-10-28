using API.Imoveis.DTOs;

namespace API.FavoritosNotificacoes.Favoritos.Interfaces
{
    public interface IFavoriteService
    {
        public bool AddFavorite(int idProperty, int idClient);

        public bool RemoveFavorite(int idProperty, int idClient);

        IEnumerable<PropertyDTO> ListFavorites(int idClient);
        
    }
}
