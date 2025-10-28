namespace API.APIs.Exceptions
{
    public class MapboxNotFoundException : Exception
    {
        public MapboxNotFoundException(string message) : base(message) { }
    }
}
