using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Proconenct.Pages.auth
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            Response.Cookies.Delete("jwtToken");
            HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
            return RedirectToPage("/auth/Login");
        }
    }
} 