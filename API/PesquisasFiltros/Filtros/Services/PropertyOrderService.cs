using API.Data;
using API.Imoveis.DTOs;
using API.Models.Enums;
using API.PesquisasFiltros.Filtros.Exceptions;
using API.PesquisasFiltros.Filtros.Interfaces;
using API.PesquisasFiltros.Pesquisas.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace API.PesquisasFiltros.Filtros.Services
{
    public class PropertyOrderService : IPropertyOrderService
    {
        private readonly LevelEstateContext context;

        public PropertyOrderService(LevelEstateContext context)
        {
            this.context = context;
        }
        public IEnumerable<PropertyListDTO> OrderProperties(IEnumerable<PropertyListDTO> properties, bool? isPriceDesc, bool? isDateDesc, bool? bestAgent)
        {
            if (properties.IsNullOrEmpty())
                throw new PropertyOrderNotFoundException("There is no properties to order!");

            //Check if status is available
            var validStatus = new[] { Status.Available };
            var filtered = properties.Where(p => validStatus.Contains(p.Status)).ToList();

            var query = filtered.AsQueryable();
            var agentIds = query.Select(p => p.AgentId).Distinct().ToList();
            var agents = context.Agents.Where(a => agentIds.Contains(a.UserId)).ToList();

            if (isPriceDesc.HasValue)
            {
                if (isPriceDesc == true)
                {
                    query = query.OrderByDescending(p => p.Price);
                }
                else
                {
                    query = query.OrderBy(p => p.Price);
                }

            }

            if (isDateDesc.HasValue)
            {
                if (isDateDesc == true)
                {
                    query = query.OrderByDescending(p => p.UpdatedAt);
                }
                else
                {
                    query = query.OrderBy(p => p.UpdatedAt);
                }

            }

            if (bestAgent.HasValue)
            {
                // Cria um dicionario com os pontos por agente
                var agentPoints = agents.ToDictionary(a => a.UserId, a => a.AnnualPoints);

                if (bestAgent == true)
                {
                    query = query.OrderByDescending(p => agentPoints.ContainsKey(p.AgentId) ? agentPoints[p.AgentId] : 0);
                }
                else
                {
                    query = query.OrderBy(p => agentPoints.ContainsKey(p.AgentId) ? agentPoints[p.AgentId] : 0);
                }
            }

            var propertiesDTO = query.Select(p => new PropertyListDTO
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
                throw new PropertyOrderNotFoundException("There is no properties!");

            return propertiesDTO.ToList();
        }
    }
}
