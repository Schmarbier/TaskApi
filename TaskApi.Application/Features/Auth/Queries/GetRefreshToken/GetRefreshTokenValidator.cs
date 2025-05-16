using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.Features.Users.Commands.RefreshToken;

namespace TaskApi.Application.Features.Auth.Queries.GetRefreshToken
{
    class GetRefreshTokenValidator : AbstractValidator<GetRefreshTokenQuery>
    {
        public GetRefreshTokenValidator()
        {
            RuleFor(x => x.Token).NotNull().NotEmpty().WithMessage("El refresh token es requerido");
        }
    }
}
