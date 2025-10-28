namespace API.APIs.Exceptions
{
    public class MapboxUnavailableException : Exception
    {
        public MapboxUnavailableException(string message) : base(message) { }
    }
}
