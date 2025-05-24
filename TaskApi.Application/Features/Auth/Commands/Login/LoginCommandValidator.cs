using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApi.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email no debe ser nulo")
                .NotNull().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no tiene un formato valido");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña no debe ser nula")
                .NotNull().WithMessage("La contraseña es requerida");
        }
    }
}
