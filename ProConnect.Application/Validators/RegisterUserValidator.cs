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
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida.")
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres.")
                .Matches(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$").WithMessage("El nombre solo puede contener letras y espacios.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido.")
                .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres.")
                .Matches(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$").WithMessage("El apellido solo puede contener letras y espacios.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9]{7,15}$").WithMessage("El telefono solo puede contener numeros y debe tener minimo 7 digitos.")
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
