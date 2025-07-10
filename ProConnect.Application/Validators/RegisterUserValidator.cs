using FluentValidation;
using ProConnect.Application.DTOs;
using ProConnect.Core.Interfaces;

namespace ProConnect.Application.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El formato del email no es válido.")
                .MustAsync(BeUniqueEmail).WithMessage("Este email ya está registrado.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida.")
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido.")
                .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("El formato del teléfono no es válido.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.UserType)
                .IsInEnum().WithMessage("El tipo de usuario no es válido.");
        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            return !await _userRepository.EmailExistsAsync(email);
        }
    }
}
