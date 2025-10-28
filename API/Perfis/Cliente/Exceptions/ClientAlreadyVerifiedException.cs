namespace API.Perfis.Cliente.Exceptions
{
    public class ClientAlreadyVerifiedException : Exception
    {
        public ClientAlreadyVerifiedException(string message) : base(message) { }
    }
}
