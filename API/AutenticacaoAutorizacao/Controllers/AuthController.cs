using API.AutenticacaoAutorizacao.DTOs;
using API.AutenticacaoAutorizacao.Exceptions;
using API.AutenticacaoAutorizacao.Interfaces;
using API.Perfis.Cliente.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.AutenticacaoAutorizacao.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RegisterDTO>> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                var result = await authService.RegisterAsync(registerDTO);

                return Ok(result);
            }
            catch (InvalidInputRegisterDTOException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("requestVerify")]
        [ProducesResponseType(typeof(RegisterDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RequestVerify([FromBody] RequestVerifyDTO dto)
        {
            try
            {
                _ = await authService.SendEmailVerificationAsync(dto.Email);
                return Ok("Verification email sent.");
            }
            catch(ClientNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (ClientAlreadyVerifiedException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status409Conflict);
            }
            catch (Exception e)
            {
                return Problem(detail: "There was a problem sending the email. Please try again later.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("verify/")]
        [ProducesResponseType(typeof(RegisterDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                await authService.VerifyAccountAsync(token);
                return Ok("Account verified successfully.");
            }
            catch (UserNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (ClientAlreadyVerifiedException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status409Conflict);
            }
            catch (SecurityTokenExpiredException)
            {
                return Problem(detail: "Verification token has expired.", statusCode: StatusCodes.Status400BadRequest);
            }
            catch (SecurityTokenException e )
            {
                return Problem(detail: "Invalid verification token." + e.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch
            {
                return Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
