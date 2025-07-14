using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Proconenct.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Solo renderizar la página, sin redirección automática
            return Page();
        }
    }
}
