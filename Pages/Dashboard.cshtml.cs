using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Proconenct.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IAuthService _authService;
        public bool IsAuthenticated { get; set; } = false;
        public bool IsClient { get; set; } = false;
        public bool IsProfessional { get; set; } = false;
        public bool IsLoading { get; set; } = true;
        public string FirstName { get; set; } = string.Empty;
        public string ProfileSummary { get; set; } = "";
        public bool IsVerified { get; set; } = false;

        public DashboardModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                IsAuthenticated = false;
                IsLoading = false;
                return Page();
            }

            var isValid = await _authService.ValidateTokenAsync(token);
            if (!isValid)
            {
                Response.Cookies.Delete("jwtToken");
                IsAuthenticated = false;
                IsLoading = false;
                return RedirectToPage("/auth/Login");
            }

            IsAuthenticated = true;
            // Obtener claims del usuario
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            // Cambiar a claim 'user_type' (así se genera en el JWT)
            var userType = jwt.Claims.FirstOrDefault(c => c.Type == "user_type")?.Value;
            FirstName = jwt.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Usuario";
            ViewData["UserName"] = FirstName;

            if (userType == "Client")
            {
                IsClient = true;
            }
            else if (userType == "Professional")
            {
                IsProfessional = true;
                // Simular datos de perfil profesional
                ProfileSummary = "Abogado laboralista con 8 años de experiencia.";
                IsVerified = jwt.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value == "true";
            }
            // Si no se detecta tipo, mostrar error
            if (!IsClient && !IsProfessional)
            {
                IsAuthenticated = false;
            }
            IsLoading = false;
            return Page();
        }
    }
} 