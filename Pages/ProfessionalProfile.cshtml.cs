using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.DTOs;
using ProConnect.Application.DTOs.Shared;
using ProConnect.Application.Interfaces;
using System.Security.Claims;

namespace Proconenct.Pages
{
    public class ProfessionalProfileModel : PageModel
    {
        private readonly IProfessionalProfileService _profileService;

        public ProfessionalProfileModel(IProfessionalProfileService profileService)
        {
            _profileService = profileService;
        }

        [BindProperty]
        public CreateProfessionalProfileDto CreateDto { get; set; } = new();

        [BindProperty]
        public UpdateProfessionalProfileDto UpdateDto { get; set; } = new();

        // Propiedades auxiliares para edición de listas como string
        [BindProperty]
        public string SpecialtiesString { get; set; } = string.Empty;
        [BindProperty]
        public string CredentialsString { get; set; } = string.Empty;
        
        // Propiedad para controlar el estado del perfil
        [BindProperty]
        public bool IsProfileActive { get; set; } = true;

        public ProfessionalProfileResponseDto? Profile { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Obtener el ID del usuario del token JWT
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/auth/Login");
                }

                // Verificar que el usuario es profesional
                if (!User.IsInRole("Professional"))
                {
                    ErrorMessage = "Solo los profesionales pueden acceder a esta pagina.";
                    return Page();
                }

                // Obtener el perfil existente si existe
                Profile = await _profileService.GetMyProfileAsync(userId);

                if (Profile != null)
                {
                    // Mapear el perfil existente al DTO de actualizacion
                    UpdateDto = new UpdateProfessionalProfileDto
                    {
                        Specialties = Profile.Specialties,
                        Bio = Profile.Bio,
                        ExperienceYears = Profile.ExperienceYears,
                        HourlyRate = Profile.HourlyRate,
                        Credentials = Profile.Credentials,
                        Location = Profile.Location,
                        Status = Profile.Status,
                        AvailabilitySchedule = Profile.AvailabilitySchedule
                    };
                    // Convertir listas a string para el formulario
                    SpecialtiesString = string.Join(", ", Profile.Specialties);
                    CredentialsString = string.Join(", ", Profile.Credentials);
                    // Mapear el estado del perfil al checkbox
                    IsProfileActive = Profile.Status == ProfileStatusDto.Active;
                }
                else
                {
                    SpecialtiesString = string.Empty;
                    CredentialsString = string.Empty;
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al cargar el perfil profesional.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/auth/Login");
                }

                if (!User.IsInRole("Professional"))
                {
                    ErrorMessage = "Solo los profesionales pueden crear perfiles.";
                    return Page();
                }

                // Convertir los strings a listas antes de guardar (robusto, sin espacios ni vacíos)
                var specialtiesList = SpecialtiesString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                var credentialsList = CredentialsString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                // Verificar si ya existe un perfil
                var existingProfile = await _profileService.GetMyProfileAsync(userId);

                if (existingProfile == null)
                {
                    // Crear nuevo perfil
                    var createDto = new CreateProfessionalProfileDto
                    {
                        Specialties = specialtiesList,
                        Bio = CreateDto.Bio,
                        ExperienceYears = CreateDto.ExperienceYears,
                        HourlyRate = CreateDto.HourlyRate,
                        Credentials = credentialsList,
                        Location = CreateDto.Location,
                        AvailabilitySchedule = CreateDto.AvailabilitySchedule
                    };

                    await _profileService.CreateProfileAsync(createDto, userId);
                    SuccessMessage = "Perfil profesional creado exitosamente.";
                }
                else
                {
                    // Actualizar perfil existente
                    var updateDto = new UpdateProfessionalProfileDto
                    {
                        Specialties = specialtiesList,
                        Bio = UpdateDto.Bio,
                        ExperienceYears = UpdateDto.ExperienceYears,
                        HourlyRate = UpdateDto.HourlyRate,
                        Credentials = credentialsList,
                        Location = UpdateDto.Location,
                        Status = IsProfileActive ? ProfileStatusDto.Active : ProfileStatusDto.Inactive,
                        AvailabilitySchedule = UpdateDto.AvailabilitySchedule
                    };

                    await _profileService.UpdateProfileAsync(existingProfile.Id, updateDto, userId);
                    SuccessMessage = "Perfil profesional actualizado exitosamente.";
                }

                // Recargar el perfil actualizado
                Profile = await _profileService.GetMyProfileAsync(userId);
                // Actualizar los strings auxiliares para mostrar en el formulario
                SpecialtiesString = string.Join(", ", Profile?.Specialties ?? new List<string>());
                CredentialsString = string.Join(", ", Profile?.Credentials ?? new List<string>());
                return Page();
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al guardar el perfil profesional.";
                return Page();
            }
        }
    }
} 