using FluentValidation;
using ProConnect.Application.DTOs;

namespace ProConnect.Application.Validators
{
    public class LoginUserValidator : AbstractValidator<LoginUserDto>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El formato del email no es válido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.");
        }
    }
}
