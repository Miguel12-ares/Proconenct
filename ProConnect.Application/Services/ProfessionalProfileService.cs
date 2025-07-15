using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;
using ProConnect.Core.Interfaces;
using System.Security.Claims;

namespace ProConnect.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para la gestión de perfiles profesionales.
    /// </summary>
    public class ProfessionalProfileService : IProfessionalProfileService
    {
        private readonly IProfessionalProfileRepository _profileRepository;
        private readonly IUserRepository _userRepository;

        public ProfessionalProfileService(
            IProfessionalProfileRepository profileRepository,
            IUserRepository userRepository)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Crea un nuevo perfil profesional.
        /// </summary>
        public async Task<ProfessionalProfileResponseDto> CreateProfileAsync(
            CreateProfessionalProfileDto createDto, 
            string userId)
        {
            // Verificar que el usuario existe y es profesional
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado");

            if (user.UserType != UserType.Professional)
                throw new InvalidOperationException("Solo los profesionales pueden crear perfiles");

            // Verificar que no tenga un perfil existente
            var existingProfile = await _profileRepository.GetByUserIdAsync(userId);
            if (existingProfile != null)
                throw new InvalidOperationException("El usuario ya tiene un perfil profesional");

            // Crear el perfil
            var profile = new ProfessionalProfile
            {
                UserId = userId,
                Specialties = createDto.Specialties,
                Bio = createDto.Bio,
                ExperienceYears = createDto.ExperienceYears,
                HourlyRate = createDto.HourlyRate,
                Credentials = createDto.Credentials,
                Location = createDto.Location,
                AvailabilitySchedule = MapAvailabilitySchedule(createDto.AvailabilitySchedule),
                Status = ProfileStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var profileId = await _profileRepository.CreateAsync(profile);

            return await MapToResponseDto(profile, user);
        }

        /// <summary>
        /// Actualiza un perfil profesional existente.
        /// </summary>
        public async Task<ProfessionalProfileResponseDto> UpdateProfileAsync(
            string profileId, 
            UpdateProfessionalProfileDto updateDto, 
            string userId)
        {
            // Verificar que el perfil existe y pertenece al usuario
            var profile = await _profileRepository.GetByIdAsync(profileId);
            if (profile == null)
                throw new InvalidOperationException("Perfil no encontrado");

            if (profile.UserId != userId)
                throw new InvalidOperationException("No tiene permisos para modificar este perfil");

            // Obtener información del usuario
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado");

            // Actualizar el perfil
            profile.Specialties = updateDto.Specialties;
            profile.Bio = updateDto.Bio;
            profile.ExperienceYears = updateDto.ExperienceYears;
            profile.HourlyRate = updateDto.HourlyRate;
            profile.Credentials = updateDto.Credentials;
            profile.Location = updateDto.Location;
            profile.AvailabilitySchedule = MapAvailabilitySchedule(updateDto.AvailabilitySchedule);
            profile.Status = (ProfileStatus)updateDto.Status;
            profile.UpdateModificationDate();

            var success = await _profileRepository.UpdateAsync(profile);
            if (!success)
                throw new InvalidOperationException("Error al actualizar el perfil");

            return await MapToResponseDto(profile, user);
        }

        /// <summary>
        /// Obtiene el perfil profesional del usuario autenticado.
        /// </summary>
        public async Task<ProfessionalProfileResponseDto?> GetMyProfileAsync(string userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                return null;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado");

            return await MapToResponseDto(profile, user);
        }

        /// <summary>
        /// Obtiene un perfil profesional público por ID.
        /// </summary>
        public async Task<ProfessionalProfileResponseDto?> GetPublicProfileAsync(string profileId)
        {
            var profile = await _profileRepository.GetByIdAsync(profileId);
            if (profile == null || profile.Status != ProfileStatus.Active)
                return null;

            var user = await _userRepository.GetByIdAsync(profile.UserId);
            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado");

            return await MapToResponseDto(profile, user);
        }

        /// <summary>
        /// Obtiene todos los perfiles profesionales activos.
        /// </summary>
        public async Task<List<ProfessionalProfileResponseDto>> GetAllActiveProfilesAsync()
        {
            var profiles = await _profileRepository.GetAllAsync();
            var activeProfiles = profiles.Where(p => p.Status == ProfileStatus.Active).ToList();

            var result = new List<ProfessionalProfileResponseDto>();
            foreach (var profile in activeProfiles)
            {
                var user = await _userRepository.GetByIdAsync(profile.UserId);
                if (user != null)
                {
                    result.Add(await MapToResponseDto(profile, user));
                }
            }

            return result;
        }

        /// <summary>
        /// Busca perfiles por especialidad.
        /// </summary>
        public async Task<List<ProfessionalProfileResponseDto>> GetProfilesBySpecialtyAsync(string specialty)
        {
            var profiles = await _profileRepository.GetBySpecialtyAsync(specialty);
            var activeProfiles = profiles.Where(p => p.Status == ProfileStatus.Active).ToList();

            var result = new List<ProfessionalProfileResponseDto>();
            foreach (var profile in activeProfiles)
            {
                var user = await _userRepository.GetByIdAsync(profile.UserId);
                if (user != null)
                {
                    result.Add(await MapToResponseDto(profile, user));
                }
            }

            return result;
        }

        /// <summary>
        /// Mapea el DTO de disponibilidad a la entidad.
        /// </summary>
        private AvailabilitySchedule MapAvailabilitySchedule(AvailabilityScheduleDto dto)
        {
            return new AvailabilitySchedule
            {
                Monday = MapDaySchedule(dto.Monday),
                Tuesday = MapDaySchedule(dto.Tuesday),
                Wednesday = MapDaySchedule(dto.Wednesday),
                Thursday = MapDaySchedule(dto.Thursday),
                Friday = MapDaySchedule(dto.Friday),
                Saturday = MapDaySchedule(dto.Saturday),
                Sunday = MapDaySchedule(dto.Sunday),
                Timezone = dto.Timezone
            };
        }

        /// <summary>
        /// Mapea el DTO de horario diario a la entidad.
        /// </summary>
        private DaySchedule MapDaySchedule(DayScheduleDto dto)
        {
            return new DaySchedule
            {
                IsAvailable = dto.IsAvailable,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                BreakStart = dto.BreakStart,
                BreakEnd = dto.BreakEnd
            };
        }

        /// <summary>
        /// Mapea la entidad a DTO de respuesta.
        /// </summary>
        private async Task<ProfessionalProfileResponseDto> MapToResponseDto(ProfessionalProfile profile, User user)
        {
            return new ProfessionalProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                Specialties = profile.Specialties,
                Bio = profile.Bio,
                ExperienceYears = profile.ExperienceYears,
                HourlyRate = profile.HourlyRate,
                Credentials = profile.Credentials,
                PortfolioItems = profile.PortfolioItems.Select(MapPortfolioItem).ToList(),
                RatingAverage = profile.RatingAverage,
                TotalReviews = profile.TotalReviews,
                Location = profile.Location,
                AvailabilitySchedule = MapAvailabilityScheduleDto(profile.AvailabilitySchedule),
                Status = (ProfileStatusDto)profile.Status,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt,
                IsCompleteForPublicView = profile.IsCompleteForPublicView(),
                UserFirstName = user.FirstName,
                UserLastName = user.LastName,
                UserEmail = user.Email
            };
        }

        /// <summary>
        /// Mapea un item del portafolio a DTO.
        /// </summary>
        private PortfolioItemDto MapPortfolioItem(PortfolioItem item)
        {
            return new PortfolioItemDto
            {
                Title = item.Title,
                Description = item.Description,
                Url = item.Url,
                ImageUrl = item.ImageUrl,
                CreatedAt = item.CreatedAt
            };
        }

        /// <summary>
        /// Mapea el horario de disponibilidad a DTO.
        /// </summary>
        private AvailabilityScheduleDto MapAvailabilityScheduleDto(AvailabilitySchedule schedule)
        {
            return new AvailabilityScheduleDto
            {
                Monday = MapDayScheduleDto(schedule.Monday),
                Tuesday = MapDayScheduleDto(schedule.Tuesday),
                Wednesday = MapDayScheduleDto(schedule.Wednesday),
                Thursday = MapDayScheduleDto(schedule.Thursday),
                Friday = MapDayScheduleDto(schedule.Friday),
                Saturday = MapDayScheduleDto(schedule.Saturday),
                Sunday = MapDayScheduleDto(schedule.Sunday),
                Timezone = schedule.Timezone
            };
        }

        /// <summary>
        /// Mapea el horario diario a DTO.
        /// </summary>
        private DayScheduleDto MapDayScheduleDto(DaySchedule schedule)
        {
            return new DayScheduleDto
            {
                IsAvailable = schedule.IsAvailable,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                BreakStart = schedule.BreakStart,
                BreakEnd = schedule.BreakEnd
            };
        }
    }
} 