namespace API.ChatVisitasFeedback.Visitas.Exceptions
{
    public class VisitDateTooSoonException : Exception
    {
        public VisitDateTooSoonException(string message) : base(message) { }
    }
}
