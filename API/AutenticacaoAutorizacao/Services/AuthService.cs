using API.AutenticacaoAutorizacao.DTOs;
using API.AutenticacaoAutorizacao.Exceptions;
using API.AutenticacaoAutorizacao.Interfaces;
using API.Data;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Interfaces;
using API.Perfis.Cliente.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace API.AutenticacaoAutorizacao.Services
{
    public class AuthService : IAuthService
    {
        public readonly LevelEstateContext context;
        public readonly IAgentService agentService;
        private static readonly Random random = new();
        private readonly IConfiguration config;
        public AuthService(LevelEstateContext context, IAgentService agentService, IConfiguration config)
        {
            this.context = context;
            this.agentService = agentService;
            this.config = config;
        }

        public string Login(string email, string password)
        {
            throw new NotImplementedException();
        }

        public bool Logout(int idUser)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RegisterAsync(RegisterDTO registerDTO)
        {
            if (string.IsNullOrEmpty(registerDTO.Email) || string.IsNullOrEmpty(registerDTO.Password) || string.IsNullOrEmpty(registerDTO.FirstName) ||
                string.IsNullOrEmpty(registerDTO.LastName))
            {
                throw new InvalidInputRegisterDTOException("First Name, Last Name, Email, and Password cannot be null or empty.");
            }

            if (registerDTO.FirstName.Length < 1 || registerDTO.FirstName.Length > 50)
                throw new InvalidInputRegisterDTOException("First Name must be between 1 and 50 characters.");

            if (registerDTO.LastName.Length < 1 || registerDTO.LastName.Length > 50)
                throw new InvalidInputRegisterDTOException("Last Name must be between 1 and 50 characters.");

            if (registerDTO.Email.Length > 255)
                throw new InvalidInputRegisterDTOException("Email cannot exceed 255 characters.");

            if (registerDTO.Password != registerDTO.ConfirmedPassword)
                throw new InvalidInputRegisterDTOException("Password and Confirm Password do not match.");

            if (context.Users.Any(u => u.Email == registerDTO.Email))
                throw new InvalidInputRegisterDTOException("This email is already in use.");

            if (registerDTO.PfpImage.IsNullOrEmpty())
                registerDTO.PfpImage = "/../Perfis/Images/defaultPfp.jpg";

            var strongPasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            if (!strongPasswordRegex.IsMatch(registerDTO.Password))
                throw new InvalidInputRegisterDTOException("Password does not meet the required criteria.");

            var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password);

            if (registerDTO.Role == Role.Agent)
            {
                if (string.IsNullOrEmpty(registerDTO.AMICode))
                    throw new InvalidInputRegisterDTOException("AMICode cannot be null or empty!");

                if (context.Agents.Any(a => a.AMICode == registerDTO.AMICode))
                    throw new InvalidInputRegisterDTOException("AMICode is already in use!");

                Agent agent = new Agent
                {
                    PfpImage = registerDTO.PfpImage,
                    FirstName = registerDTO.FirstName,
                    LastName = registerDTO.LastName,
                    Phone = registerDTO.Phone,
                    Email = registerDTO.Email,
                    Password = encryptedPassword,
                    Role = Role.Agent,
                    Status = UserStatus.NonVerified,
                    RegistrationDate = DateTime.UtcNow,
                    AMICode = registerDTO.AMICode,
                    ReferralCode = GenerateUniqueReferralCode(),
                    Visits = new List<Visit>(),
                    Chats = new List<Chat>(),
                    Badges = new List<Badge>(),
                    Properties = new List<Property>(),
                    MonthlyPoints = 0,
                    AnnualPoints = 0,
                    AllMoneyEarned = 0,
                    MoneyEarnedLastMonth = 0,
                };

                await context.Users.AddAsync(agent);
                await context.SaveChangesAsync();

                await SendEmailVerificationAsync(registerDTO.Email);

                return true;
            }
            else if (registerDTO.Role == Role.Client)
            {
                Client client = new Client
                {
                    PfpImage = registerDTO.PfpImage,
                    FirstName = registerDTO.FirstName,
                    LastName = registerDTO.LastName,
                    Phone = registerDTO.Phone,
                    Email = registerDTO.Email,
                    Password = encryptedPassword,
                    Role = Role.Client,
                    Status = UserStatus.NonVerified,
                    RegistrationDate = DateTime.UtcNow,
                    ReferralCodeUsed = registerDTO.ReferralCode,
                    Visits = new List<Visit>(),
                    Chats = new List<Chat>(),
                    Notifications = new List<Notification>(),
                    Favorite = new List<Property>()
                };

                await context.Users.AddAsync(client);
                await context.SaveChangesAsync();

                await SendEmailVerificationAsync(registerDTO.Email);

                return true;
            }

            return false;
        }

        public async Task VerifyAccountAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["JwtSettings:Key"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidIssuer = config["JwtSettings:Issuer"],
                ValidAudience = config["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero
            };

            var tokenValid = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var emailInToken = tokenValid.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailInToken))
                throw new SecurityTokenException("Token does not contain a valid email claim.");

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == emailInToken);
            if(user == null)
                throw new UserNotFoundException("No user found");

            if (user.Status == UserStatus.Verified)
                throw new ClientAlreadyVerifiedException("User is already verified.");

            user.Status = UserStatus.Verified;
            await context.SaveChangesAsync();

            if (user.Role == Role.Client)
            {
                agentService.ApplyAgentRefferalPoints(user.UserId);
            }
        }

        public async Task<string> SendEmailVerificationAsync(string email)
        {
            var user = context.Users
                .Select(u => new { u.FirstName, u.LastName, u.Email, u.Status })
                .Where(r => r.Email == email)
                .FirstOrDefault();

            if (user == null)
                throw new ClientNotFoundException("Client not found");

            if (user.Status.Equals(UserStatus.Verified))
                throw new ClientAlreadyVerifiedException("Client already verified");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var exp = now.AddMinutes(30);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(
                issuer: config["JwtSettings:Issuer"],
                audience: config["JwtSettings:Audience"],
                claims: claims,
                notBefore: now,
                expires: exp,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var verificationLink = $"https://levelestate.pt/api/auth/verify?token={Uri.EscapeDataString(jwt)}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("LevelEstate", config["Smtp:Email"]));
            message.To.Add(new MailboxAddress(user.FirstName + " " + user.LastName, email));
            message.Subject = "Verify your email (LevelEstate)";

            message.Body = new TextPart("html")
            {
                Text = $@"
                <p>Hello {user.FirstName} {user.LastName},</p>
                <p>Click the link below to verify your email:</p>
                <p><a href=""{verificationLink}"">Verify Email</a></p>
                <p>If you didn’t request this, please ignore this message.</p>"
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(config["Smtp:Email"], config["Smtp:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return jwt;
        }

        public bool VerifyTokenAgent(string token)
        {
            throw new NotImplementedException();
        }

        public bool VerifyTokenClient(string token)
        {
            throw new NotImplementedException();
        }

        private string GenerateUniqueReferralCode()
        {
            string code;
            do
            {
                code = random.Next(100000, 999999).ToString();
            } while (context.Agents.Any(a => a.ReferralCode == code));

            return code;
        }
    }
}
