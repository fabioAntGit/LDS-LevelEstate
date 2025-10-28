using API.Models.Enums;
using API.Perfis.Agente.DTOs;

namespace API.Perfis.Agente.Interfaces
{
    public interface IAgentService
    {
        public Task<bool> ConvertPoints(bool annual);
        public bool AddPoints(int idAgent, int points);
        public void ResetMonthlyPoints(int idAgent);
        public void ReserYearlyPoints(int idAgent);
        public ICollection<AgentListDTO> GetAgents(int? position, TypeRanking? typeRanking, string? name);
        public AgentDTO GetProfileDetails(int idAgent);
        public bool ApplyAgentRefferalPoints(int idClient);
    }
}
