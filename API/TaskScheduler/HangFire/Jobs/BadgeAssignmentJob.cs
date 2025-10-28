using API.RankingsBadges.Badges.Interfaces;

namespace API.TaskScheduler.HangFire.Jobs
{
    public class BadgeAssignmentJob
    {
        private readonly IBadgeService badgeService;

        public BadgeAssignmentJob (IBadgeService badgeService)
        {
            this.badgeService = badgeService;
        }

        public async Task Execute()
        {
            var now = DateTime.Now;

            if (now.Month == 1)
                await badgeService.Create(true);
            else
                await badgeService.Create(false);
        }
    }
}
