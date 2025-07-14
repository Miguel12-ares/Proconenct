using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Added for CookieOptions
using System; // Added for DateTimeOffset

namespace Proconenct.Pages.auth
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            // Eliminar la cookie jwtToken exactamente igual que en login
            Response.Cookies.Delete("jwtToken");
            HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
            return Redirect("/auth/Login");
        }
    }
} 