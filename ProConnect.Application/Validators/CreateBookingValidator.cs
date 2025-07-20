using FluentValidation;
using ProConnect.Application.DTOs;

namespace ProConnect.Application.Validators
{
    /// <summary>
    /// Validador para la creación de reservas
    /// </summary>
    public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingValidator()
        {
            RuleFor(x => x.ProfessionalId)
                .NotEmpty().WithMessage("El ID del profesional es requerido")
                .Matches(@"^[0-9a-fA-F]{24}$").WithMessage("El ID del profesional debe ser un ObjectId válido");

            RuleFor(x => x.AppointmentDate)
                .NotEmpty().WithMessage("La fecha de la cita es requerida")
                .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de la cita no puede estar en el pasado")
                .LessThan(DateTime.UtcNow.AddYears(1)).WithMessage("La fecha de la cita no puede ser más de un año en el futuro");

            RuleFor(x => x.Duration)
                .InclusiveBetween(15, 480).WithMessage("La duración debe estar entre 15 y 480 minutos");

            RuleFor(x => x.ConsultationType)
                .NotEmpty().WithMessage("El tipo de consulta es requerido")
                .Must(BeValidConsultationType).WithMessage("El tipo de consulta debe ser: Presencial, Virtual o Telefonica");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Las notas especiales no pueden exceder 1000 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.ClientPhone)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("El formato del teléfono no es válido")
                .When(x => !string.IsNullOrEmpty(x.ClientPhone));

            RuleFor(x => x.ClientEmail)
                .EmailAddress().WithMessage("El formato del email no es válido")
                .When(x => !string.IsNullOrEmpty(x.ClientEmail));
        }

        private bool BeValidConsultationType(string consultationType)
        {
            return new[] { "Presencial", "Virtual", "Telefonica" }.Contains(consultationType, StringComparer.OrdinalIgnoreCase);
        }
    }
} 