using API.Data;
using API.Models.Enums;
using API.Perfis.Agente.DTOs;
using API.Perfis.Agente.Exceptions;
using API.Perfis.Agente.Interfaces;
using API.Perfis.Cliente.Exceptions;
using API.RankingsBadges.Badges.DTOs;
using API.RankingsBadges.Badges.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Perfis.Agente.Services
{
    public class AgentService : IAgentService
    {
        private readonly LevelEstateContext context;

        public AgentService(LevelEstateContext context)
        {
            this.context = context;
        }

        public bool AddPoints(int idAgent, int points)
        {
            if (points <= 0)
            {
                throw new InvalidPointsException($"{points} is an invalid number of points");
            }

            var agent = context.Agents.FirstOrDefault(a => a.UserId == idAgent);

            if (agent == null)
            {
                throw new AgentNotFoundException($"Agent with id {idAgent} not found");
            }

            if (agent.Status == UserStatus.NonVerified)
            {
                throw new AgentNotVerifiedException($"Agent with id {idAgent} is not verified");
            }

            agent.MonthlyPoints += points;
            agent.AnnualPoints += points;

            // TO DO: CHAMAR METODO DOS RANKINGS

            context.SaveChanges();

            return true;
        }

        public async Task<bool> ConvertPoints(bool annual)
        {
            var agents = annual
                ? await context.Agents.ToListAsync()
                : await context.Agents.Where(a => a.MonthlyPoints > 0).ToListAsync();

            if (!agents.Any())
                return false;

            foreach (var agent in agents)
            {
                double moneyConverted = agent.MonthlyPoints * 0.50;

                if (annual)
                {
                    agent.AnnualPoints = 0;
                }

                agent.MonthlyPoints = 0;
                agent.AllMoneyEarned = agent.AllMoneyEarned + moneyConverted;
                agent.MoneyEarnedLastMonth = moneyConverted;
            }

            await context.SaveChangesAsync();

            return true;
        }

        public ICollection<AgentListDTO> GetAgents(int? position, TypeRanking? typeRanking, string? name)
        {
            var query = context.Agents.Include(a => a.Badges).Where(a => a.Status == UserStatus.Verified).AsQueryable();

            if (query.IsNullOrEmpty())
                throw new AgentNotFoundException("There is no agents verified!");

            if (position.HasValue && (position != 1 && position != 2 && position != 3))
                throw new BadgePositionNotFoundException($"There is no badge for position: {position}, only positions 1, 2 or 3 are allowed!");

            if (!string.IsNullOrWhiteSpace(name))
            {
                var parts = name.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var agentsList = query.ToList();//Execute in memory so it can apply filter!

                agentsList = agentsList.Where(agent =>
                             parts.Any(part => agent.FirstName.ToLower().Contains(part) || agent.LastName.ToLower().Contains(part))).ToList();

                if (agentsList.IsNullOrEmpty())
                    throw new AgentNotFoundException("There is no agents verified!");

                query = agentsList.AsQueryable();
            }

            if (position.HasValue)
            {
                query = query.Where(a => a.Badges.Any(b => b.Position == position.Value));
            }

            if (typeRanking.HasValue)
            {
                switch (typeRanking.Value)
                {
                    case TypeRanking.Monthly:
                        query = query.OrderByDescending(a => a.MonthlyPoints);
                        break;
                    case TypeRanking.Yearly:
                        query = query.OrderByDescending(a => a.AnnualPoints);
                        break;
                }
            }

            var agentListDTO = query.Select(a => new AgentListDTO
            {
                AgentId = a.UserId,
                FirstName = a.FirstName,
                LastName = a.LastName,
                PfpImage = a.PfpImage,
                MonthlyPoints = a.MonthlyPoints,
                AnnualPoints = a.AnnualPoints,
                Badges = a.Badges.Select(b => new BadgeDTO
                {
                    BadgeId = b.BadgeId,
                    Position = b.Position,
                    Month = b.Month,
                    Year = b.Year
                }).ToList()
            }).ToList();

            if (agentListDTO.IsNullOrEmpty())
                throw new AgentNotFoundException("There is no agents for these filters!");

            return agentListDTO;
        }

        public AgentDTO GetProfileDetails(int idAgent)
        {
            throw new NotImplementedException();
        }

        public void ReserYearlyPoints(int idAgent)
        {
            throw new NotImplementedException();
        }

        public void ResetMonthlyPoints(int idAgent)
        {
            throw new NotImplementedException();
        }

        public bool ApplyAgentRefferalPoints(int idClient)
        {
            const int ReferralCodePoints = 5;

            var client = context.Clients
                .Where(c => c.UserId == idClient)
                .Select(c => new { c.Status, c.ReferralCodeUsed })
                .AsNoTracking()
                .FirstOrDefault();

            if (client == null)
            {
                throw new ClientNotFoundException("Client not found");
            }

            if (client.Status == UserStatus.NonVerified)
            {
                throw new ClientNotVerifiedException("Client not verified");
            }

            if (string.IsNullOrEmpty(client.ReferralCodeUsed))
            {
                return false;
            }

            var agent = context.Agents
                .Where(a => a.ReferralCode.Equals(client.ReferralCodeUsed))
                .Select(a => new { a.UserId })
                .AsNoTracking()
                .FirstOrDefault();

            if (agent == null)
            {
                throw new AgentNotFoundException($"Agent with Referral Code {client.ReferralCodeUsed} not found");
            }

            return AddPoints(agent.UserId, ReferralCodePoints);
        }
    }
}
