using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.Common.Interfaces;
using TaskApi.Domain.Entities;

namespace TaskApi.Application.Features.Auth.Queries.GetRefreshToken
{
    class GetRefreshTokenHandler : IRequestHandler<GetRefreshTokenQuery, RefreshToken>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public GetRefreshTokenHandler(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<RefreshToken> Handle(GetRefreshTokenQuery request, CancellationToken cancellationToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(request.Token, cancellationToken);

            if(token == null)
            {
                throw new UnauthorizedAccessException("Refresh token inválido");
            }

            return token;
        }
    }
}
