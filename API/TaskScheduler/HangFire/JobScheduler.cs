using Hangfire;
using API.TaskScheduler.HangFire.Jobs;

namespace API.TaskScheduler.HangFire
{
    public class JobScheduler
    {
        public static void MonthlyJobs()
        {
            string cron = "0 0 1 * *";

            RecurringJob.AddOrUpdate<ConvertPointsJob>(
                "ConvertPointsToMoney",
                job => job.Execute(),
                cron);

            RecurringJob.AddOrUpdate<BadgeAssignmentJob>(
                "AssignBadgeToAgent",
                job => job.Execute(),
                cron);

            RecurringJob.AddOrUpdate<CreateRankingJob>(
                "CreateRanking",
                job => job.Execute(),
                cron);
        }
    }
}
 