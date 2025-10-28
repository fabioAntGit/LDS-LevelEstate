namespace API.Imoveis.Exceptions
{
    public class InvalidPropertyDataException : Exception
    {
        public InvalidPropertyDataException(string message): base(message) { }
    }
}
