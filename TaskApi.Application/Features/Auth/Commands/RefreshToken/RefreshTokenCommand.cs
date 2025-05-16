using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.DTOs;

namespace TaskApi.Application.Features.Users.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<AuthenticationResult>
    {
        public string RefreshToken { get; set; } = default!;
    }
}
