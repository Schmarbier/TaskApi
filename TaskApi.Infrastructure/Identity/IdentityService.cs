using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.Common.Interfaces;
using TaskApi.Application.Interfaces;
using TaskApi.Domain.Entities;
using TaskApi.Infrastructure.Persistence;

namespace TaskApi.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public IdentityService(ApplicationDbContext context, ITokenService tokenService)
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

            // Crear hash de la contraseña
            var passwordHash = HashPassword(password);

            // Crear nuevo usuario
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

            // Generar token JWT
            var token = _tokenService.GenerateJwtToken(user);
            
            // Generar refresh token
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Añadir refresh token al usuario
            user.RefreshTokens.Add(refreshToken);

            // Remover refresh tokens antiguos (opcional)
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

            // Revocar el token actual
            existingRefreshToken.RevokedAt = DateTime.UtcNow;
            existingRefreshToken.ReplacedByToken = refreshToken;

            // Generar nuevo refresh token
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            // Eliminar tokens antiguos
            RemoveOldRefreshTokens(user);

            await _context.SaveChangesAsync();

            // Generar nuevo JWT
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

            // Revocar token
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private void RemoveOldRefreshTokens(User user, int daysToKeep = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            //user.RefreshTokens.RemoveRange(x =>
            //    (x.Revoked != null && x.Expires <= cutoffDate) ||
            //    (x.Revoked == null && x.IsExpired));
        }

        private string HashPassword(string password)
        {
            // Para un entorno de producción, deberías usar un algoritmo más seguro como Bcrypt o PBKDF2
            // Esta es una implementación simple para desarrollo
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            // Combinar salt y hash
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
            var salt = new byte[128];
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
