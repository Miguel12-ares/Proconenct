using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using ProConnect.Core.Entities;

namespace Proconenct.Pages.auth
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        [BindProperty]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string PhoneNumber { get; set; } = string.Empty;

        [BindProperty]
        public string DocumentId { get; set; } = string.Empty;

        [BindProperty]
        public int DocumentType { get; set; }

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        public int UserType { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden.";
                return Page();
            }

            try
            {
                var registerDto = new RegisterUserDto
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.ToLowerInvariant().Trim(),
                    PhoneNumber = PhoneNumber.Trim(),
                    DocumentId = DocumentId.Trim(),
                    DocumentType = (DocumentType)DocumentType,
                    Password = Password,
                    ConfirmPassword = ConfirmPassword, // Aseguramos que se envía
                    UserType = (ProConnect.Core.Entities.UserType)UserType
                };

                var result = await _authService.RegisterAsync(registerDto);

                if (result.Success)
                {
                    // Redirigir al login con mensaje de éxito
                    return RedirectToPage("/auth/Login", new { registered = true });
                }
                else
                {
                    ErrorMessage = result.Errors?.FirstOrDefault() ?? "Error al registrar usuario";
                    return Page();
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Error interno del servidor. Inténtalo de nuevo.";
                return Page();
            }
        }
    }
} 