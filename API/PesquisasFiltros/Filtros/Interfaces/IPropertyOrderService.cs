using API.PesquisasFiltros.Pesquisas.DTOs;

namespace API.PesquisasFiltros.Filtros.Interfaces
{
    public interface IPropertyOrderService
    {
        public IEnumerable<PropertyListDTO> OrderProperties(IEnumerable<PropertyListDTO> properties, bool? isPriceDesc, bool? isDateDesc, bool? bestAgent);
    }
}
