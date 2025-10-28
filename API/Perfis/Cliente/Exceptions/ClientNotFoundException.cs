namespace API.Perfis.Cliente.Exceptions
{
    public class ClientNotFoundException : Exception
    {
        public ClientNotFoundException(string message) : base(message) { }
    }
}
