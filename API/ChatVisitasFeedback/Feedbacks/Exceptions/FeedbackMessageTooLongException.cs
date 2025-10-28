namespace API.ChatVisitasFeedback.Feedbacks.Exceptions
{
    public class FeedbackMessageTooLongException : Exception
    {
        public FeedbackMessageTooLongException(string message) : base(message) { }
    }
}
