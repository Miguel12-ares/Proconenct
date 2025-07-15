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

            return await MapToResponseDto(profile, user, isPublicView: false);
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

            return await MapToResponseDto(profile, user, isPublicView: false);
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

            return await MapToResponseDto(profile, user, isPublicView: true);
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

            return await MapToResponseDto(profile, user, isPublicView: false);
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
                    result.Add(await MapToResponseDto(profile, user, isPublicView: true));
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
                    result.Add(await MapToResponseDto(profile, user, isPublicView: true));
                }
            }

            return result;
        }

        /// <summary>
        /// Actualiza el horario de disponibilidad del profesional.
        /// </summary>
        public async Task<bool> UpdateAvailabilityScheduleAsync(string userId, AvailabilityScheduleDto scheduleDto)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new InvalidOperationException("Perfil profesional no encontrado");

            profile.AvailabilitySchedule = MapAvailabilitySchedule(scheduleDto);
            profile.UpdateModificationDate();
            return await _profileRepository.UpdateAsync(profile);
        }

        /// <summary>
        /// Agrega un bloqueo de disponibilidad para el profesional.
        /// </summary>
        public async Task<AvailabilityBlockDto> AddAvailabilityBlockAsync(string userId, CreateAvailabilityBlockDto blockDto)
        {
            if (blockDto.StartDate > blockDto.EndDate)
                throw new InvalidOperationException("La fecha de inicio debe ser anterior o igual a la de fin");

            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new InvalidOperationException("Perfil profesional no encontrado");

            // Validar overlapping
            if (profile.AvailabilityBlocks.Any(b =>
                (blockDto.StartDate <= b.EndDate && blockDto.EndDate >= b.StartDate)))
            {
                throw new InvalidOperationException("El rango de fechas se solapa con un bloqueo existente");
            }

            var block = new AvailabilityBlock
            {
                StartDate = blockDto.StartDate.Date,
                EndDate = blockDto.EndDate.Date,
                Reason = blockDto.Reason
            };
            profile.AvailabilityBlocks.Add(block);
            profile.UpdateModificationDate();
            await _profileRepository.UpdateAsync(profile);

            return new AvailabilityBlockDto
            {
                Id = block.Id,
                StartDate = block.StartDate,
                EndDate = block.EndDate,
                Reason = block.Reason
            };
        }

        /// <summary>
        /// Obtiene todos los bloqueos de disponibilidad del profesional.
        /// </summary>
        public async Task<List<AvailabilityBlockDto>> GetAvailabilityBlocksAsync(string userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new InvalidOperationException("Perfil profesional no encontrado");

            return profile.AvailabilityBlocks.Select(b => new AvailabilityBlockDto
            {
                Id = b.Id,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Reason = b.Reason
            }).ToList();
        }

        /// <summary>
        /// Elimina un bloqueo de disponibilidad por id.
        /// </summary>
        public async Task<bool> DeleteAvailabilityBlockAsync(string userId, string blockId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new InvalidOperationException("Perfil profesional no encontrado");

            var block = profile.AvailabilityBlocks.FirstOrDefault(b => b.Id == blockId);
            if (block == null)
                throw new InvalidOperationException("Bloqueo no encontrado");

            profile.AvailabilityBlocks.Remove(block);
            profile.UpdateModificationDate();
            return await _profileRepository.UpdateAsync(profile);
        }

        /// <summary>
        /// Consulta los slots disponibles para una fecha específica.
        /// </summary>
        public async Task<AvailabilityCheckResponseDto> CheckAvailabilityAsync(string userId, DateTime date)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new InvalidOperationException("Perfil profesional no encontrado");

            // Verificar si la fecha está bloqueada
            if (profile.AvailabilityBlocks.Any(b => date.Date >= b.StartDate.Date && date.Date <= b.EndDate.Date))
            {
                return new AvailabilityCheckResponseDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    AvailableSlots = new List<AvailableSlotDto>()
                };
            }

            // Obtener el día de la semana
            var dayOfWeek = date.DayOfWeek;
            DaySchedule? daySchedule = dayOfWeek switch
            {
                DayOfWeek.Monday => profile.AvailabilitySchedule.Monday,
                DayOfWeek.Tuesday => profile.AvailabilitySchedule.Tuesday,
                DayOfWeek.Wednesday => profile.AvailabilitySchedule.Wednesday,
                DayOfWeek.Thursday => profile.AvailabilitySchedule.Thursday,
                DayOfWeek.Friday => profile.AvailabilitySchedule.Friday,
                DayOfWeek.Saturday => profile.AvailabilitySchedule.Saturday,
                DayOfWeek.Sunday => profile.AvailabilitySchedule.Sunday,
                _ => null
            };

            var slots = new List<AvailableSlotDto>();
            if (daySchedule != null && daySchedule.IsAvailable)
            {
                slots.Add(new AvailableSlotDto
                {
                    StartTime = daySchedule.StartTime,
                    EndTime = daySchedule.EndTime
                });
            }

            return new AvailabilityCheckResponseDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                AvailableSlots = slots
            };
        }

        public async Task<bool> AddServiceAsync(string userId, CreateServiceDto dto)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new InvalidOperationException("Perfil profesional no encontrado");
            if (profile.Services.Count >= 10)
                throw new InvalidOperationException("No se pueden agregar mas de 10 servicios");
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("El nombre del servicio es obligatorio");
            if (dto.Price <= 0)
                throw new InvalidOperationException("El precio debe ser mayor a 0");
            if (dto.EstimatedDurationMinutes <= 0 || dto.EstimatedDurationMinutes > 1440)
                throw new InvalidOperationException("La duracion estimada debe ser mayor a 0 y menor a 24 horas");
            var service = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Type = (ServiceType)dto.Type,
                Price = dto.Price,
                EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                IsActive = true
            };
            return await _profileRepository.AddServiceAsync(userId, service);
        }

        public async Task<bool> UpdateServiceAsync(string userId, UpdateServiceDto dto)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new InvalidOperationException("Perfil profesional no encontrado");
            var service = profile.Services.FirstOrDefault(s => s.Id == dto.Id);
            if (service == null)
                throw new InvalidOperationException("Servicio no encontrado");
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("El nombre del servicio es obligatorio");
            if (dto.Price <= 0)
                throw new InvalidOperationException("El precio debe ser mayor a 0");
            if (dto.EstimatedDurationMinutes <= 0 || dto.EstimatedDurationMinutes > 1440)
                throw new InvalidOperationException("La duracion estimada debe ser mayor a 0 y menor a 24 horas");
            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Type = (ServiceType)dto.Type;
            service.Price = dto.Price;
            service.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
            service.IsActive = dto.IsActive;
            return await _profileRepository.UpdateServiceAsync(userId, service);
        }

        public async Task<bool> DeleteServiceAsync(string userId, string serviceId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new InvalidOperationException("Perfil profesional no encontrado");
            var service = profile.Services.FirstOrDefault(s => s.Id == serviceId);
            if (service == null)
                throw new InvalidOperationException("Servicio no encontrado");
            return await _profileRepository.DeleteServiceAsync(userId, serviceId);
        }

        public async Task<List<ServiceDto>> GetServicesAsync(string userId)
        {
            var services = await _profileRepository.GetServicesAsync(userId);
            return services.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Type = (ServiceTypeDto)s.Type,
                Price = s.Price,
                EstimatedDurationMinutes = s.EstimatedDurationMinutes,
                IsActive = s.IsActive
            }).ToList();
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
        private async Task<ProfessionalProfileResponseDto> MapToResponseDto(ProfessionalProfile profile, User user, bool isPublicView = false)
        {
            var responseDto = new ProfessionalProfileResponseDto
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
                IsPublicView = isPublicView
            };

            // En vista pública, filtrar información sensible
            if (isPublicView)
            {
                // No incluir información personal del usuario
                responseDto.UserFirstName = null;
                responseDto.UserLastName = null;
                responseDto.UserEmail = null;
                
                // Solo mostrar nombre completo como string
                responseDto.UserFirstName = $"{user.FirstName} {user.LastName}";
            }
            else
            {
                // En vista privada, incluir toda la información
                responseDto.UserFirstName = user.FirstName;
                responseDto.UserLastName = user.LastName;
                responseDto.UserEmail = user.Email;
            }

            return responseDto;
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