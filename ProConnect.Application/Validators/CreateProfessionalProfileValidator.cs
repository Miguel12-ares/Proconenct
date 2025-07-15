using FluentValidation;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;

namespace ProConnect.Application.Validators
{
    /// <summary>
    /// Validador para la creación de perfiles profesionales.
    /// </summary>
    public class CreateProfessionalProfileValidator : AbstractValidator<CreateProfessionalProfileDto>
    {
        public CreateProfessionalProfileValidator()
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

            // Validar horario de disponibilidad
            RuleFor(x => x.AvailabilitySchedule)
                .SetValidator(new AvailabilityScheduleValidator());
        }
    }

    /// <summary>
    /// Validador para el horario de disponibilidad.
    /// </summary>
    public class AvailabilityScheduleValidator : AbstractValidator<AvailabilityScheduleDto>
    {
        public AvailabilityScheduleValidator()
        {
            RuleFor(x => x.Timezone)
                .NotEmpty().WithMessage("La zona horaria es obligatoria");

            RuleFor(x => x.Monday).SetValidator(new DayScheduleValidator());
            RuleFor(x => x.Tuesday).SetValidator(new DayScheduleValidator());
            RuleFor(x => x.Wednesday).SetValidator(new DayScheduleValidator());
            RuleFor(x => x.Thursday).SetValidator(new DayScheduleValidator());
            RuleFor(x => x.Friday).SetValidator(new DayScheduleValidator());
            RuleFor(x => x.Saturday).SetValidator(new DayScheduleValidator());
            RuleFor(x => x.Sunday).SetValidator(new DayScheduleValidator());
        }
    }

    /// <summary>
    /// Validador para el horario de un día específico.
    /// </summary>
    public class DayScheduleValidator : AbstractValidator<DayScheduleDto>
    {
        public DayScheduleValidator()
        {
            When(x => x.IsAvailable, () =>
            {
                RuleFor(x => x.StartTime)
                    .NotEmpty().WithMessage("La hora de inicio es obligatoria cuando el día está disponible")
                    .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").WithMessage("Formato de hora inválido (HH:MM)");

                RuleFor(x => x.EndTime)
                    .NotEmpty().WithMessage("La hora de fin es obligatoria cuando el día está disponible")
                    .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").WithMessage("Formato de hora inválido (HH:MM)");

                RuleFor(x => x)
                    .Must(day => IsValidTimeRange(day.StartTime, day.EndTime))
                    .WithMessage("La hora de fin debe ser posterior a la hora de inicio");
            });
        }

        private bool IsValidTimeRange(string startTime, string endTime)
        {
            if (!TimeSpan.TryParse(startTime, out var start) || !TimeSpan.TryParse(endTime, out var end))
                return false;

            return end > start;
        }
    }
} 