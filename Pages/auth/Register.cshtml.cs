using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

public class RegisterModel : PageModel
{
    [BindProperty]
    [Required]
    public string FirstName { get; set; }

    [BindProperty]
    [Required]
    public string LastName { get; set; }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [BindProperty]
    [Required]
    public string PhoneNumber { get; set; }

    [BindProperty]
    [Required]
    public int UserType { get; set; }

    [BindProperty]
    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    [BindProperty]
    [Required]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor, completa todos los campos correctamente.";
            return Page();
        }

        var registerDto = new
        {
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            PhoneNumber = PhoneNumber,
            UserType = UserType,
            Password = Password,
            ConfirmPassword = ConfirmPassword
        };

        using var client = new HttpClient();
        var json = JsonSerializer.Serialize(registerDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("http://localhost:5089/api/auth/register", content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            SuccessMessage = "¡Registro exitoso! Ahora puedes iniciar sesión.";
            return RedirectToPage("/auth/Login", new { registered = true });
        }
        else
        {
            try
            {
                var jsonDoc = JsonNode.Parse(responseString);
                var errors = jsonDoc?["errors"]?.ToString();
                if (!string.IsNullOrEmpty(errors))
                {
                    ErrorMessage = Regex.Unescape(errors.Replace("[", "").Replace("]", "").Replace("\"", "")).Trim();
                    if (ErrorMessage.ToLower().Contains("email ya registrado") || ErrorMessage.ToLower().Contains("ya existe"))
                        ErrorMessage = "El correo electrónico ya está registrado. Usa otro o inicia sesión.";
                }
                else
                {
                    ErrorMessage = "Error en el registro. Intenta nuevamente.";
                }
            }
            catch
            {
                ErrorMessage = "Error en el registro. Intenta nuevamente.";
            }
            return Page();
        }
    }
} 