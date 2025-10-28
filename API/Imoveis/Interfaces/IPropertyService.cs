using API.Imoveis.DTOs;
using API.Models;
using API.PesquisasFiltros.Pesquisas.DTOs;

namespace API.Imoveis.Interfaces
{
    public interface IPropertyService
    {
        public PropertyDTO CreateProperty(CreatePropertyDTO property, int userId);
        public Task<PropertyDTO> UpdatePropertyAsync(UpdatePropertyDTO property, int UserId);
        public bool DeleteProperty(int idProperty, int userId);
        public PropertyDTO GetPropertyDetails(int idProperty);
        public IEnumerable<PropertyListDTO> GetPropertiesOf(int idAgent);
    }
}
