using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.Common.Interfaces;
using TaskApi.Application.DTOs;
using TaskApi.Application.Interfaces;
using TaskApi.Domain.Entities;

namespace TaskApi.Application.Features.Users.Commands.RefreshToken
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthenticationResult>
    {
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ICurrentUserService _currentUserService;
        public RefreshTokenHandler(ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository, ICurrentUserService currentUserService)
        {
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _currentUserService = currentUserService;
        }

        public async Task<AuthenticationResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {

            var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

            if (existingToken == null || !existingToken.IsActive)
                throw new UnauthorizedAccessException("Refresh token inválido");

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            newRefreshToken.CreatedByIp = _currentUserService.IpAddress;
            newRefreshToken.UserAgent = _currentUserService.UserAgent;

            existingToken.ReplacedByToken = newRefreshToken.Token;
            existingToken.RevokedAt = DateTime.UtcNow;
            
            existingToken.User.RefreshTokens.Add(newRefreshToken);

            await _refreshTokenRepository.SaveAsync(cancellationToken);

            var accessToken = _tokenService.GenerateJwtToken(existingToken.User);

            return new AuthenticationResult
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };
        }
    }
}
