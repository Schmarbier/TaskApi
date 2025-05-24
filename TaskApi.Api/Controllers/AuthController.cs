using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using TaskApi.Application.Features.Auth.Commands.Login;
using TaskApi.Application.Features.Users.Commands.RefreshToken;
using TaskApi.Application.Interfaces;

namespace TaskApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<AuthController> _logger;
        private readonly ISender _mediator;
        private readonly IHttpContextAccessor _httpContext;

        public AuthController(IIdentityService identityService, ILogger<AuthController> logger, ISender mediator, IHttpContextAccessor httpContext)
        {
            _identityService = identityService;
            _logger = logger;
            _mediator = mediator;
            _httpContext = httpContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _identityService.CreateUserAsync(
                request.Username,
                request.Email,
                request.Password,
                request.IsAdmin ? "Admin" : "User");

            if (!result.Success)
            {
                return BadRequest(new { message = "Error al registrar el usuario. El email o username ya está en uso." });
            }

            return Ok(new { userId = result.UserId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand request)
        {
            return Ok(await _mediator.Send(request));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command )
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "Refresh token no proporcionado" });
            }

            var result = await _identityService.RevokeRefreshTokenAsync(refreshToken);

            if (!result)
            {
                return NotFound(new { message = "Token no encontrado" });
            }

            return Ok(new { message = "Token revocado" });
        }

        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict,
                Secure = true // Solo establecer a true en producción con HTTPS
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
