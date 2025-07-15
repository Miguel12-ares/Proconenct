using FluentValidation;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;

namespace ProConnect.Application.Validators
{
    /// <summary>
    /// Validador para la actualización de perfiles profesionales.
    /// </summary>
    public class UpdateProfessionalProfileValidator : AbstractValidator<UpdateProfessionalProfileDto>
    {
        public UpdateProfessionalProfileValidator()
        {
            RuleFor(x => x.Specialties)
                .NotEmpty().WithMessage("Debe seleccionar al menos una especialidad")
                .Must(specialties => specialties != null && specialties.Count > 0)
                .WithMessage("Debe tener al menos una especialidad");

            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("La biografía es obligatoria")
                .MinimumLength(100).WithMessage("La biografía debe tener al menos 100 caracteres")
                .MaximumLength(2000).WithMessage("La biografía no puede exceder 2000 caracteres");

            RuleFor(x => x.ExperienceYears)
                .InclusiveBetween(0, 50).WithMessage("Los años de experiencia deben estar entre 0 y 50");

            RuleFor(x => x.HourlyRate)
                .GreaterThan(0).WithMessage("La tarifa por hora debe ser mayor a 0")
                .LessThanOrEqualTo(10000).WithMessage("La tarifa por hora no puede exceder $10,000");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("La ubicación es obligatoria")
                .MinimumLength(3).WithMessage("La ubicación debe tener al menos 3 caracteres")
                .MaximumLength(200).WithMessage("La ubicación no puede exceder 200 caracteres");

            RuleFor(x => x.Credentials)
                .Must(credentials => credentials == null || credentials.Count <= 20)
                .WithMessage("No puede tener más de 20 credenciales");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Estado de perfil inválido");

            // Validar horario de disponibilidad
            RuleFor(x => x.AvailabilitySchedule)
                .SetValidator(new AvailabilityScheduleValidator());
        }
    }
} 