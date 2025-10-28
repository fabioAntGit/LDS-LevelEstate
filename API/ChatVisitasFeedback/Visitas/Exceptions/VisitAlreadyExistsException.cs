namespace API.ChatVisitasFeedback.Visitas.Exceptions
{
    public class VisitAlreadyExistsException : Exception
    {
        public VisitAlreadyExistsException(string message) : base(message) { }
    }
}
