using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApi.Application.Features.Users.Commands.RefreshToken
{
    public class GetRefreshtokenValidator : AbstractValidator<GetRefreshTokenCommand>
    {
        public GetRefreshtokenValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty()
                .WithMessage("El refresh token es requerido");
        }
    }
}
