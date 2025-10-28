using API.AutenticacaoAutorizacao.DTOs;

namespace API.AutenticacaoAutorizacao.Interfaces
{
    public interface IAuthService
    {
        public Task<bool> RegisterAsync(RegisterDTO registerDTO);
        public string Login(string email, string password);
        public bool Logout(int idUser);
        public Task VerifyAccountAsync(string token);
        public Task<string> SendEmailVerificationAsync(string email);
        public bool VerifyTokenClient(string token);
        public bool VerifyTokenAgent(string token);
    }
}
