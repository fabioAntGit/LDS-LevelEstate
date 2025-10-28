namespace API.Perfis.Agente.Exceptions
{
    public class AgentNotAuthorizedException : Exception
    {
        public AgentNotAuthorizedException(string message) : base(message) { }
    }
}
