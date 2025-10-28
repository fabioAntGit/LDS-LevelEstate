using API.Data;
using API.Models.Enums;
using API.Perfis.Agente.Interfaces;
using API.RankingsBadges.Badges.Interfaces;
using API.RankingsBadges.Rankings.Interfaces;
using API.TaskScheduler.HangFire;
using API.TaskScheduler.HangFire.Jobs;
using FakeItEasy;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.TaskScheduler
{
    public class JobSchedulerTest
    {

        [Fact]
        public void MonthlyJobsConvertPointsJob_WhenCalled_ShouldRegisterRecurringJobInStorage()
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            JobScheduler.MonthlyJobs();

            var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();

            var convertJob = recurringJobs.FirstOrDefault(j => j.Id == "ConvertPointsToMoney");

            Assert.NotNull(convertJob);
            Assert.Equal("0 0 1 * *", convertJob.Cron);
            Assert.Contains("ConvertPointsJob", convertJob.Job.Type.Name);

            //limpo o job do storage para garantir o isolmento entre testes
            RecurringJob.RemoveIfExists("ConvertPointsToMoney");
        }

        [Fact]
        public void MonthlyJobsBadgeAssignmentJob_WhenCalled_ShouldRegisterRecurringJobInStorage()
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            JobScheduler.MonthlyJobs();

            var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();

            var convertJob = recurringJobs.FirstOrDefault(j => j.Id == "AssignBadgeToAgent");

            Assert.NotNull(convertJob);
            Assert.Equal("0 0 1 * *", convertJob.Cron);
            Assert.Contains("BadgeAssignmentJob", convertJob.Job.Type.Name);

            //limpo o job do storage para garantir o isolmento entre testes
            RecurringJob.RemoveIfExists("AssignBadgeToAgent");
        }

        [Fact]
        public void MonthlyJobsCreateRankingJob_WhenCalled_ShouldRegisterRecurringJobInStorage()
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            JobScheduler.MonthlyJobs();

            var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();

            var convertJob = recurringJobs.FirstOrDefault(j => j.Id == "CreateRanking");

            Assert.NotNull(convertJob);
            Assert.Equal("0 0 1 * *", convertJob.Cron);
            Assert.Contains("CreateRankingJob", convertJob.Job.Type.Name);

            //limpo o job do storage para garantir o isolmento entre testes
            RecurringJob.RemoveIfExists("CreateRanking");
        }

        [Fact]
        public async Task ConvertPointsJobExecute_WhenJobRuns_ShouldCallAgentServiceConvertPoints()
        {
            var fakeAgentService = A.Fake<IAgentService>();
            var job = new ConvertPointsJob(fakeAgentService);

            await job.Execute();

            A.CallTo(() => fakeAgentService.ConvertPoints(false))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task BadgeAssignmentJobExecute_WhenJobRuns_ShouldCallBadgeServiceCreate()
        {
            var fakeBadgeService = A.Fake<IBadgeService>();
            var job = new BadgeAssignmentJob(fakeBadgeService);

            await job.Execute();

            A.CallTo(() => fakeBadgeService.Create(false))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CreateRankingJobExecute_WhenJobRuns_ShouldCallRankingServiceCreate()
        {
            var fakeRankingService = A.Fake<IRankingService>();
            var job = new CreateRankingJob(fakeRankingService);

            await job.Execute();

            A.CallTo(() => fakeRankingService.CreateRanking(false)).MustHaveHappenedOnceExactly();
        }
    }
}
