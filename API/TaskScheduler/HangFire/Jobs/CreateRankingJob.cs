using API.Perfis.Agente.Services;
using API.RankingsBadges.Rankings.Interfaces;

namespace API.TaskScheduler.HangFire.Jobs
{
    public class CreateRankingJob
    {
        private readonly IRankingService rankingService;

        public CreateRankingJob(IRankingService rankingService)
        {
            this.rankingService = rankingService;
        }

        public async Task Execute()
        {
            var now = DateTime.UtcNow;

            if (now.Month == 1)
                await rankingService.CreateRanking(true);
            else
                await rankingService.CreateRanking(false);
        }
    }
}
