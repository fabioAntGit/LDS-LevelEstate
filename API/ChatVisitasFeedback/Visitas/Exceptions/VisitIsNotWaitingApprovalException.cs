namespace API.ChatVisitasFeedback.Visitas.Exceptions
{
    public class VisitIsNotWaitingApprovalException : Exception
    {
        public VisitIsNotWaitingApprovalException(string message) : base(message) { }
    }
}
