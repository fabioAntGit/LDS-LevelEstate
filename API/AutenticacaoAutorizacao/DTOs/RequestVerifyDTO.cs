using System.ComponentModel.DataAnnotations;

namespace API.AutenticacaoAutorizacao.DTOs
{
    public class RequestVerifyDTO
    {
        [Required]
        public string Email { get; set; }
    }
}
