namespace API.ChatVisitasFeedback.Chat.Exceptions
{
    public class UnauthorizedChatAccessException : Exception
    {
        public UnauthorizedChatAccessException(string message): base(message) { }
    }
}
