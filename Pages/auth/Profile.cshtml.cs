using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Proconenct.Pages.auth
{
    public class ProfileModel : PageModel
    {
        private readonly IAuthService _authService;

        [BindProperty]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        public string Phone { get; set; } = string.Empty;

        [BindProperty]
        public string DocumentId { get; set; } = string.Empty;

        [BindProperty]
        public int DocumentType { get; set; }

        [BindProperty]
        public string Bio { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public ProfileModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Obtener el token de la cookie
                var token = Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/auth/Login");
                }

                // Validar el token
                var isValid = await _authService.ValidateTokenAsync(token);
                if (!isValid)
                {
                    Response.Cookies.Delete("jwtToken");
                    return RedirectToPage("/auth/Login");
                }

                // Extraer el userId del token (esto requeriría decodificar el JWT)
                // Por ahora, usaremos un método alternativo
                var userId = ExtractUserIdFromToken(token);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/auth/Login");
                }

                // Obtener el perfil del usuario
                var profile = await _authService.GetProfileAsync(userId);
                if (profile == null)
                {
                    ErrorMessage = "No se pudo cargar el perfil del usuario.";
                    return Page();
                }

                // Llenar los campos del formulario
                FirstName = profile.FirstName;
                LastName = profile.LastName;
                Phone = profile.PhoneNumber;
                DocumentId = profile.DocumentId;
                DocumentType = (int)profile.DocumentType;
                Bio = profile.Bio;
                Email = profile.Email;

                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "Error interno del servidor. Inténtalo de nuevo.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Obtener el token de la cookie
                var token = Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/auth/Login");
                }

                // Validar el token
                var isValid = await _authService.ValidateTokenAsync(token);
                if (!isValid)
                {
                    Response.Cookies.Delete("jwtToken");
                    return RedirectToPage("/auth/Login");
                }

                // Extraer el userId del token
                var userId = ExtractUserIdFromToken(token);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/auth/Login");
                }

                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || 
                    string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(DocumentId))
                {
                    ErrorMessage = "Los campos Nombre, Apellido, Teléfono y Número de Documento son obligatorios.";
                    return Page();
                }

                // Validar formato del documento
                if (DocumentId.Length < 5 || !DocumentId.All(char.IsDigit))
                {
                    ErrorMessage = "El número de documento debe tener mínimo 5 dígitos y solo contener números.";
                    return Page();
                }

                // Crear el DTO para actualizar
                var updateDto = new UpdateUserProfileDto
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    PhoneNumber = Phone.Trim(),
                    DocumentId = DocumentId.Trim(),
                    DocumentType = (ProConnect.Core.Entities.DocumentType)DocumentType,
                    Bio = Bio?.Trim() ?? string.Empty
                };

                // Actualizar el perfil
                var result = await _authService.UpdateProfileAsync(userId, updateDto);

                if (result)
                {
                    SuccessMessage = "Perfil actualizado exitosamente.";
                    // Recargar los datos para mostrar la información actualizada
                    var profile = await _authService.GetProfileAsync(userId);
                    if (profile != null)
                    {
                        Email = profile.Email;
                    }
                }
                else
                {
                    ErrorMessage = "Error al actualizar el perfil. Inténtalo de nuevo.";
                }

                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "Error interno del servidor. Inténtalo de nuevo.";
                return Page();
            }
        }

        private string ExtractUserIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "id");
                return userIdClaim?.Value ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
} 
