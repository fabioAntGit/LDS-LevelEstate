#nullable disable
using API.Imoveis.DTOs;
using API.Models.Enums;

namespace API.PesquisasFiltros.Pesquisas.DTOs
{
    public class PropertyListDTO
    {
        public int PropertyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public Topologies Topology { get; set; }
        public ICollection<PictureDTO> Pictures { get; set; }
        public Status Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int AgentId { get; set; }
    }
}
