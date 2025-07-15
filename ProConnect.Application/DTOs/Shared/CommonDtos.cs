namespace ProConnect.Application.DTOs.Shared
{
    /// <summary>
    /// DTO para el horario de disponibilidad.
    /// </summary>
    public class AvailabilityScheduleDto
    {
        public DayScheduleDto Monday { get; set; } = new();
        public DayScheduleDto Tuesday { get; set; } = new();
        public DayScheduleDto Wednesday { get; set; } = new();
        public DayScheduleDto Thursday { get; set; } = new();
        public DayScheduleDto Friday { get; set; } = new();
        public DayScheduleDto Saturday { get; set; } = new();
        public DayScheduleDto Sunday { get; set; } = new();
        public string Timezone { get; set; } = "UTC";
    }

    /// <summary>
    /// DTO para el horario de un día específico.
    /// </summary>
    public class DayScheduleDto
    {
        public bool IsAvailable { get; set; } = false;
        public string StartTime { get; set; } = "09:00";
        public string EndTime { get; set; } = "17:00";
        public string? BreakStart { get; set; }
        public string? BreakEnd { get; set; }
    }

    /// <summary>
    /// DTO para el estado del perfil.
    /// </summary>
    public enum ProfileStatusDto
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Suspended = 3
    }

    /// <summary>
    /// DTO para un bloqueo de disponibilidad.
    /// </summary>
    public class AvailabilityBlockDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO para crear un bloqueo de disponibilidad.
    /// </summary>
    public class CreateAvailabilityBlockDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO para la respuesta de consulta de disponibilidad (slots disponibles para una fecha específica).
    /// </summary>
    public class AvailabilityCheckResponseDto
    {
        public string Date { get; set; } = string.Empty; // formato YYYY-MM-DD
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
    }

    /// <summary>
    /// DTO para un slot disponible.
    /// </summary>
    public class AvailableSlotDto
    {
        public string StartTime { get; set; } = string.Empty; // formato HH:MM
        public string EndTime { get; set; } = string.Empty;   // formato HH:MM
    }

    /// <summary>
    /// DTO para eliminar un bloqueo de disponibilidad por id.
    /// </summary>
    public class DeleteAvailabilityBlockDto
    {
        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para un servicio ofrecido por el profesional.
    /// </summary>
    public class ServiceDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ServiceTypeDto Type { get; set; }
        public decimal Price { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO para crear un servicio.
    /// </summary>
    public class CreateServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ServiceTypeDto Type { get; set; }
        public decimal Price { get; set; }
        public int EstimatedDurationMinutes { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un servicio.
    /// </summary>
    public class UpdateServiceDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ServiceTypeDto Type { get; set; }
        public decimal Price { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Enum para tipos de tarifa de servicio.
    /// </summary>
    public enum ServiceTypeDto
    {
        Hourly = 0,
        PerSession = 1,
        Fixed = 2
    }
} 