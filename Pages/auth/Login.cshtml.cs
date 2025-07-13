using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using System.Text.Json;

namespace Proconenct.Pages.auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public LoginModel(IAuthService authService, IHttpClientFactory httpClientFactory)
        {
            _authService = authService;
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet(bool? registered, int? verified)
        {
            if (registered == true)
            {
                SuccessMessage = "¡Registro exitoso! Por favor, verifica tu correo electrónico antes de iniciar sesión.";
            }

            if (verified == 1)
            {
                SuccessMessage = "¡Email verificado exitosamente! Ya puedes iniciar sesión.";
            }
            else if (verified == 0)
            {
                ErrorMessage = "Error al verificar el email. El enlace puede haber expirado o ser inválido.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var loginDto = new LoginUserDto
                {
                    Email = Email,
                    Password = Password
                };

                var result = await _authService.LoginAsync(loginDto);

                if (result.Success)
                {
                    // Guardar token en cookie
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // Cambiar a true en producción con HTTPS
                        SameSite = SameSiteMode.Strict,
                        MaxAge = TimeSpan.FromHours(1)
                    };

                    Response.Cookies.Append("jwtToken", result.Token!, cookieOptions);

                    // Redirigir a la landing page protegida
                    return RedirectToPage("/Home");
                }
                else
                {
                    ErrorMessage = result.Errors?.FirstOrDefault() ?? "Error al iniciar sesión";
                    return Page();
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Error interno del servidor. Inténtalo de nuevo.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostResendVerificationAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Por favor, ingresa tu correo electrónico.";
                return Page();
            }

            try
            {
                var result = await _authService.SendEmailVerificationAsync(Email);
                if (result)
                {
                    SuccessMessage = "Se ha enviado un nuevo enlace de verificación a tu correo electrónico.";
                }
                else
                {
                    ErrorMessage = "No se pudo enviar el enlace de verificación. Verifica tu correo electrónico.";
                }
            }
            catch
            {
                ErrorMessage = "Error interno del servidor. Inténtalo de nuevo.";
            }

            return Page();
        }
    }
} 