using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using TaskApi.Application.Interfaces;
using TaskApi.Domain.Entities;
using TaskApi.Infraestructure.Data;

namespace TaskApi.Infraestructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly ApiDbContext _context;
        private readonly TokenService _tokenService;

        public IdentityService(ApiDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<(bool Success, string UserId)> CreateUserAsync(string username, string email, string password, string role)
        {
            // Verificar si el usuario ya existe
            if (await _context.Users.AnyAsync(u => u.Email == email || u.Username == username))
            {
                return (false, string.Empty);
            }

            var passwordHash = HashPassword(password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, user.Id.ToString());
        }

        public async Task<(bool Success, string UserId, string Token, string RefreshToken)> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return (false, string.Empty, string.Empty, string.Empty);
            }

            var token = _tokenService.GenerateJwtToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(refreshToken);

            RemoveOldRefreshTokens(user);

            await _context.SaveChangesAsync();

            return (true, user.Id.ToString(), token, refreshToken.Token);
        }

        public async Task<(bool Success, string UserId, string Token, string RefreshToken)> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (false, string.Empty, string.Empty, string.Empty);
            }

            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Id == Guid.Parse(userId));

            if (user == null)
            {
                return (false, string.Empty, string.Empty, string.Empty);
            }

            var existingRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);

            if (existingRefreshToken == null || !existingRefreshToken.IsActive)
            {
                return (false, string.Empty, string.Empty, string.Empty);
            }

            existingRefreshToken.Revoked = DateTime.UtcNow;
            existingRefreshToken.ReplacedByToken = refreshToken;

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            RemoveOldRefreshTokens(user);

            await _context.SaveChangesAsync();

            var newToken = _tokenService.GenerateJwtToken(user);

            return (true, user.Id.ToString(), newToken, newRefreshToken.Token);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            if (user == null)
            {
                return false;
            }

            var token = user.RefreshTokens.Single(x => x.Token == refreshToken);

            if (!token.IsActive)
            {
                return false;
            }

            token.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private void RemoveOldRefreshTokens(User user, int daysToKeep = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            user.RefreshTokens.RemoveAll(x =>
                (x.Revoked != null && x.Expires <= cutoffDate) ||
                (x.Revoked == null && x.IsExpired));
        }

        private string HashPassword(string password)
        {
            // TODO: Deberia implementar un algoritmo más seguro como Bcrypt o PBKDF2
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            var hashBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, hashBytes, 0, salt.Length);
            Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // Convertir el hash almacenado de nuevo a bytes
            var hashBytes = Convert.FromBase64String(storedHash);

            // Extraer el salt (los primeros 64 bytes)
            var salt = new byte[64];
            Array.Copy(hashBytes, 0, salt, 0, salt.Length);

            // Usar el mismo algoritmo con el mismo salt para verificar
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            // Comparar los hashes
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != hashBytes[salt.Length + i])
                    return false;
            }

            return true;
        }
    }
}
