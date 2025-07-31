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

            // RuleFor(x => x.Password)
            //     .NotEmpty().WithMessage("La contrasena es requerida.")
            //     .MinimumLength(8).WithMessage("La contrasena debe tener minimo 8 caracteres.")
            //     .Must((user, password) => {
            //         var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&._-])[A-Za-z\\d@$!%*?&._-]{8,}$");
            //         var result = regex.IsMatch((password ?? "").Trim());
            //         if (!result)
            //         {
            //             Console.WriteLine($"[VALIDACION] Password no cumple regex: '{password}'");
            //         }
            //         return result;
            //     }).WithMessage("Debe tener mayuscula, minuscula, numero y caracter especial");

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

            RuleFor(x => x.DocumentId)
                .NotEmpty().WithMessage("El número de documento es requerido.")
                .MinimumLength(5).WithMessage("El número de documento debe tener mínimo 5 dígitos.")
                .MaximumLength(20).WithMessage("El número de documento no puede exceder 20 caracteres.")
                .Matches(@"^[0-9]+$").WithMessage("El número de documento solo puede contener números.")
                .MustAsync(BeUniqueDocumentId).WithMessage("Este número de documento ya está registrado.");

            RuleFor(x => x.DocumentType)
                .IsInEnum().WithMessage("El tipo de documento no es válido.");

            RuleFor(x => x.UserType)
                .IsInEnum().WithMessage("El tipo de usuario no es válido.");
        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            return !await _userRepository.EmailExistsAsync(email);
        }

        private async Task<bool> BeUniqueDocumentId(string documentId, CancellationToken cancellationToken)
        {
            return !await _userRepository.DocumentIdExistsAsync(documentId);
        }
    }
}
