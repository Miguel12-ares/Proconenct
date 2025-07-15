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
} 