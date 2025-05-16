using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.Common.Interfaces;
using TaskApi.Application.DTOs;
using TaskApi.Application.Features.Auth.Queries.GetRefreshToken;
using TaskApi.Application.Interfaces;
using TaskApi.Domain.Entities;

namespace TaskApi.Application.Features.Users.Commands.RefreshToken
{
    public class GetRefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthenticationResult>
    {
        private readonly ITokenService _tokenService;
        private readonly ISender _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public GetRefreshTokenHandler(IMediator mediator, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _currentUserService = currentUserService;
        }

        public async Task<AuthenticationResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {

            var existingToken = await _mediator.Send(new GetRefreshTokenQuery(request.RefreshToken), cancellationToken);

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
