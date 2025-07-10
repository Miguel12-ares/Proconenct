using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

public class LoginModel : PageModel
{
    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [BindProperty]
    [Required]
    public string Password { get; set; }

    [BindProperty]
    public bool RememberMe { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    public void OnGet(bool? registered)
    {
        if (registered == true)
        {
            SuccessMessage = "¡Registro exitoso! Ahora puedes iniciar sesión.";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor, completa todos los campos correctamente.";
            return Page();
        }

        var loginDto = new
        {
            email = Email,
            password = Password,
            rememberMe = RememberMe
        };

        using var client = new HttpClient();
        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("http://localhost:5089/api/auth/login", content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            SuccessMessage = "¡Inicio de sesión exitoso!";
            return RedirectToPage("/Index");
        }
        else
        {
            try
            {
                var jsonDoc = JsonNode.Parse(responseString);
                var errors = jsonDoc?["errors"]?.ToString();
                if (!string.IsNullOrEmpty(errors))
                {
                    // Decodificar unicode y limpiar el mensaje
                    ErrorMessage = Regex.Unescape(errors.Replace("[", "").Replace("]", "").Replace("\"", "")).Trim();
                    if (ErrorMessage.Contains("Credenciales inválidas"))
                        ErrorMessage = "Correo o contraseña incorrectos.";
                }
                else
                {
                    ErrorMessage = "Error en el inicio de sesión. Intenta nuevamente.";
                }
            }
            catch
            {
                ErrorMessage = "Error en el inicio de sesión. Intenta nuevamente.";
            }
            return Page();
        }
    }
} 