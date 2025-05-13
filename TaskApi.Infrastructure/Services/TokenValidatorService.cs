using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.Common.Interfaces;
using TaskApi.Application.DTOs;
using TaskApi.Domain.Entities;
using TaskApi.Infrastructure.Persistence;

namespace TaskApi.Infrastructure.Services
{
    public class TokenValidatorService : ITokenValidatorService
    {
        private readonly ApplicationDbContext _context;

        public TokenValidatorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TokenValidationResult> ValidateAsync(TokenValidationContext context)
        {
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            //var token = context.SecurityToken as JwtSecurityToken;

            //if (userId == null)
            //{
            //    return TokenValidationResult.Fail("Token inválido.");
            //}

            var user = await _context.Users.Include(x => x.RefreshTokens).FirstAsync(u => u.Id == Guid.Parse(userId));
            var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.IsActive);
            if (user == null || refreshToken == null)
            {
                return TokenValidationResult.Fail("Token revocado.");
            }

            return TokenValidationResult.Success();
        }
    }
}
