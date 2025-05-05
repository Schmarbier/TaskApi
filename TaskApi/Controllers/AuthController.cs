using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskApi.Application.Interfaces;
using TaskApi.Infraestructure.Domain.Entities;
using TaskApi.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskApi.Application.DTOs;
using Microsoft.AspNetCore.Identity.Data;
using LoginRequest = TaskApi.Application.DTOs.LoginRequest;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskApi.Application.Responses;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ApiDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ApiDbContext context, IConfiguration config, ILogger<AuthController> logger)
        {
            _authService = authService;
            _context = context;
            _config = config;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for user: {Email}", request.Email);
            
            var user = await _context.Usuarios.SingleOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return Unauthorized();

            var hasher = new PasswordHasher<Usuario>();
            var result = hasher.VerifyHashedPassword(user, user.Password, request.Password);

            if (result == PasswordVerificationResult.Failed) return Unauthorized();

            var accessToken = _authService.GenerarToken(user);
            var refreshToken = _authService.GenerarRefreshToken();

            user.Token = accessToken;
            user.RefreshToken = refreshToken;
            var tokenExpryTime = DateTime.Now.AddDays(7);
            user.RefreshTokenExpiryTime = tokenExpryTime;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Login successful for user: {Email}", user.Email);

            return Ok(new
            {
                token = accessToken,
                refreshToken = refreshToken,
                tokenExpire = tokenExpryTime
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existe = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
            if (existe) return BadRequest("El email ya está registrado.");

            var user = new Usuario
            {
                Email = request.Email
            };

            var hasher = new PasswordHasher<Usuario>();
            user.Password = hasher.HashPassword(user, request.Password);

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado correctamente.");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if(ModelState.IsValid == false) return BadRequest(ModelState);

            if (!IsJwt(request.Token))
            {
                return BadRequest("El token no tiene un formato JWT válido.");
            }

            var principal = GetPrincipalFromExpiredToken(request.Token);
            var email = principal?.Identity?.Name;

            var user = await _context.Usuarios.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null ||
                user.RefreshToken != request.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return Unauthorized("Token inválido o expirado");
            }

            var newAccessToken = _authService.GenerarToken(user);
            var newRefreshToken = _authService.GenerarRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Ok(new 
            {
                token = newAccessToken,
                refreshToken = newRefreshToken
            }));
        }

        private bool IsJwt(string token)
        {
            return !string.IsNullOrWhiteSpace(token)
                && token.Count(c => c == '.') == 2;
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                ValidateLifetime = false // clave: permitir tokens expirados
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Token inválido");
            }

            return principal;
        }

    }
}
