using API.Perfis.Agente.Interfaces;


namespace API.TaskScheduler.HangFire.Jobs
{
    public class ConvertPointsJob
    {
        private readonly IAgentService agentService;

        public ConvertPointsJob(IAgentService agentService)
        {
            this.agentService = agentService;
        }

        public async Task Execute()
        {
            var now = DateTime.Now;

            if (now.Month == 1)
                await agentService.ConvertPoints(true);
            else
                await agentService.ConvertPoints(false);
        }
    }
}
