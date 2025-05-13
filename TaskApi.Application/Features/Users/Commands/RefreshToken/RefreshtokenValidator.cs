using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApi.Application.Features.Users.Commands.RefreshToken
{
    public class RefreshtokenValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshtokenValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty()
                .WithMessage("El refresh token es requerido");
        }
    }
}
