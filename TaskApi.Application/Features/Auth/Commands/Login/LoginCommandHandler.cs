using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.Features.Auth.DTOs;
using TaskApi.Application.Interfaces;

namespace TaskApi.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IIdentityService _identityService;

        public LoginCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.LoginAsync(request.Email, request.Password);

            if (!result.Success)
            {
                throw new UnauthorizedAccessException("Email o contraseña incorrectos");
            }

            return new LoginResult
            {
                UserId = result.UserId,
                AccessToken = result.Token,
                RefreshToken = result.RefreshToken
            };
        }
    }
}
