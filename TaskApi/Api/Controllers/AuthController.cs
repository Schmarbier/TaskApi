using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskApi.Application.Interfaces;
using TaskApi.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskApi.Application.DTOs;
using Microsoft.AspNetCore.Identity.Data;
using LoginRequest = TaskApi.Application.DTOs.LoginRequest;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskApi.Api.Responses;
using TaskApi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Serilog.Context;

namespace TaskApi.Api.Controllers
{
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


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IIdentityService identityService, ILogger<AuthController> logger)
        {
            _identityService = identityService;
            _logger = logger;
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
        public async Task<IActionResult> Login(LoginRequest request)
        {
            using (LogContext.PushProperty("UserEmail", request.Email))
            using (LogContext.PushProperty("Action", "Login"))
            {
                _logger.LogInformation("Intento de login para usuario {UserEmail}", request.Email);

                var result = await _identityService.LoginAsync(request.Email, request.Password);

                if (!result.Success)
                {
                    return BadRequest(new { message = "Email o contraseña incorrectos" });
                }

                SetRefreshTokenInCookie(result.RefreshToken);

                return Ok(new
                {
                    userId = result.UserId,
                    token = result.Token,
                    refreshToken = result.RefreshToken
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token o refresh token no proporcionados" });
            }

            var result = await _identityService.RefreshTokenAsync(token, refreshToken);

            if (!result.Success)
            {
                return Unauthorized(new { message = "Token inválido o expirado" });
            }

            SetRefreshTokenInCookie(result.RefreshToken);

            return Ok(new
            {
                userId = result.UserId,
                token = result.Token,
                refreshToken = result.RefreshToken
            });
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

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginRequest request)
        //{
        //    _logger.LogInformation("Login attempt for user: {Email}", request.Email);

        //    var user = await _context.Usuarios.SingleOrDefaultAsync(u => u.Email == request.Email);
        //    if (user == null) return Unauthorized();

        //    var hasher = new PasswordHasher<User>();
        //    var result = hasher.VerifyHashedPassword(user, user.Password, request.Password);

        //    if (result == PasswordVerificationResult.Failed) return Unauthorized();

        //    var accessToken = _authService.GenerarToken(user);
        //    var refreshToken = _authService.GenerarRefreshToken();

        //    user.Token = accessToken;
        //    user.RefreshToken = refreshToken;
        //    var tokenExpryTime = DateTime.Now.AddDays(7);
        //    user.RefreshTokenExpiryTime = tokenExpryTime;
        //    await _context.SaveChangesAsync();

        //    _logger.LogInformation("Login successful for user: {Email}", user.Email);

        //    return Ok(new
        //    {
        //        token = accessToken,
        //        refreshToken,
        //        tokenExpire = tokenExpryTime
        //    });
        //}

        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        //{
        //    var existe = await _context.Users.AnyAsync(u => u.Email == request.Email);
        //    if (existe) return BadRequest("El email ya está registrado.");

        //    var user = new User
        //    {
        //        Email = request.Email
        //    };

        //    var hasher = new PasswordHasher<User>();
        //    user.PasswordHash = hasher.HashPassword(user, request.Password);

        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    return Ok("Usuario registrado correctamente.");
        //}

        //[HttpPost("refresh")]
        //public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        //{
        //    if(ModelState.IsValid == false) return BadRequest(ModelState);

        //    if (!IsJwt(request.Token))
        //    {
        //        return BadRequest("El token no tiene un formato JWT válido.");
        //    }

        //    var principal = GetPrincipalFromExpiredToken(request.Token);
        //    var email = principal?.Identity?.Name;

        //    var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == email);
        //    if (user == null ||
        //        user.RefreshToken != request.RefreshToken ||
        //        user.RefreshTokenExpiryTime <= DateTime.Now)
        //    {
        //        return Unauthorized("Token inválido o expirado");
        //    }

        //    var newAccessToken = _authService.GenerarToken(user);
        //    var newRefreshToken = _authService.GenerarRefreshToken();

        //    user.RefreshToken = newRefreshToken;
        //    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
        //    await _context.SaveChangesAsync();

        //    return Ok(ApiResponse.Ok(new 
        //    {
        //        token = newAccessToken,
        //        refreshToken = newRefreshToken
        //    }));
        //}

        //private bool IsJwt(string token)
        //{
        //    return !string.IsNullOrWhiteSpace(token)
        //        && token.Count(c => c == '.') == 2;
        //}

        //private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        //{
        //    var tokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuer = true,
        //        ValidIssuer = _config["Jwt:Issuer"],
        //        ValidateAudience = true,
        //        ValidAudience = _config["Jwt:Audience"],
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(
        //            Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
        //        ValidateLifetime = false // clave: permitir tokens expirados
        //    };

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        //    if (securityToken is not JwtSecurityToken jwtSecurityToken ||
        //        !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        throw new SecurityTokenException("Token inválido");
        //    }

        //    return principal;
        //}

    }
}
