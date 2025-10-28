namespace API.ChatVisitasFeedback.Feedbacks.Exceptions
{
    public class FeedbackAlreadyExistsException : Exception
    {
        public FeedbackAlreadyExistsException(string message) : base(message) { }
    }
}
