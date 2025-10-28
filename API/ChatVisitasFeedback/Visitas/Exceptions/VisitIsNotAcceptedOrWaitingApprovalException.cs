namespace API.ChatVisitasFeedback.Visitas.Exceptions
{
    public class VisitIsNotAcceptedOrWaitingApprovalException : Exception
    {
        public VisitIsNotAcceptedOrWaitingApprovalException(string message) : base(message) { }
    }
}
