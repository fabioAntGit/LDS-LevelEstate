using API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.AutenticacaoAutorizacao.DTOs
{
    public class RegisterDTO
    {
        public string? PfpImage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format!")]
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmedPassword { get; set; }
        public Role Role { get; set; }
        public string? AMICode { get; set; }
        public string? ReferralCode { get; set; }
    }
}
