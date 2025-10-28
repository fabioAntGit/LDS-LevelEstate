namespace API.ChatVisitasFeedback.Visitas.Exceptions
{
    public class VisitCanceledAfterLimitException : Exception
    {
        public VisitCanceledAfterLimitException(string message) : base(message) { }
    }
}
