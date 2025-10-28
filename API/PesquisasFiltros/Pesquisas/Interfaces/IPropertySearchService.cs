using API.Models.Enums;
using API.PesquisasFiltros.Pesquisas.DTOs;

namespace API.PesquisasFiltros.Pesquisas.Interfaces
{
    public interface IPropertySearchService
    {
        public IEnumerable<PropertyListDTO> GetPropertiesByFilter(Topologies? topology, double? minPrice, double? maxPrice, TypeOperations? typeOperation, double? latitude, double? longitude);

    }
}
