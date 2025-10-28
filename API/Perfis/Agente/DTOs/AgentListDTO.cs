using API.Models;
using API.RankingsBadges.Badges.DTOs;

namespace API.Perfis.Agente.DTOs
{
    public class AgentListDTO
    {
        public int AgentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PfpImage { get; set; }
        public ICollection<BadgeDTO> Badges { get; set; }
        public int MonthlyPoints { get; set; }
        public int AnnualPoints { get; set; }
    }
}
