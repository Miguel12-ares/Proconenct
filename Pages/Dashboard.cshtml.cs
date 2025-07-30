using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.Interfaces;
using ProConnect.Application.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Proconenct.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IBookingService _bookingService;
        private readonly IReviewService _reviewService;
        private readonly IProfessionalProfileService _profileService;
        private readonly IUserService _userService;

        public bool IsAuthenticated { get; set; } = false;
        public bool IsClient { get; set; } = false;
        public bool IsProfessional { get; set; } = false;
        public bool IsLoading { get; set; } = true;
        public string FirstName { get; set; } = string.Empty;
        public string ProfileSummary { get; set; } = "";
        public bool IsVerified { get; set; } = false;

        // Datos para profesionales
        public ProfessionalProfileResponseDto? ProfessionalProfile { get; set; }
        public List<BookingDto> MyBookings { get; set; } = new();
        public List<BookingDto> ReceivedBookings { get; set; } = new();
        public List<ReviewDto> MyReviews { get; set; } = new();
        public DashboardStats Stats { get; set; } = new();

        public DashboardModel(
            IAuthService authService,
            IBookingService bookingService,
            IReviewService reviewService,
            IProfessionalProfileService profileService,
            IUserService userService)
        {
            _authService = authService;
            _bookingService = bookingService;
            _reviewService = reviewService;
            _profileService = profileService;
            _userService = userService;
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
            var userType = jwt.Claims.FirstOrDefault(c => c.Type == "user_type")?.Value;
            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            FirstName = jwt.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Usuario";
            ViewData["UserName"] = FirstName;

            if (userType == "Client")
            {
                IsClient = true;
                await LoadClientData(userId);
            }
            else if (userType == "Professional")
            {
                IsProfessional = true;
                await LoadProfessionalData(userId);
            }

            if (!IsClient && !IsProfessional)
            {
                IsAuthenticated = false;
            }

            IsLoading = false;
            return Page();
        }

        private async Task LoadClientData(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                // Cargar reservas del cliente
                var bookings = await _bookingService.GetBookingsByClientAsync(userId, 10, 0);
                MyBookings = bookings ?? new List<BookingDto>();
            }
            catch
            {
                MyBookings = new List<BookingDto>();
            }
        }

        private async Task LoadProfessionalData(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                // Cargar perfil profesional
                var profile = await _profileService.GetMyProfileAsync(userId);
                ProfessionalProfile = profile;

                if (profile != null)
                {
                    ProfileSummary = $"{profile.Specialties.FirstOrDefault() ?? "Profesional"} con {profile.ExperienceYears} años de experiencia.";
                    IsVerified = profile.Status == ProConnect.Application.DTOs.Shared.ProfileStatusDto.Active;
                }

                // Cargar reservas recibidas (como profesional)
                var receivedBookings = await _bookingService.GetBookingsByProfessionalAsync(userId, 10, 0);
                ReceivedBookings = receivedBookings ?? new List<BookingDto>();

                // Cargar reseñas recibidas
                var reviews = await _reviewService.GetByProfessionalIdAsync(userId);
                MyReviews = reviews ?? new List<ReviewDto>();

                // Calcular estadísticas
                await CalculateStats(userId);
            }
            catch
            {
                ProfessionalProfile = null;
                ReceivedBookings = new List<BookingDto>();
                MyReviews = new List<ReviewDto>();
                Stats = new DashboardStats();
            }
        }

        private async Task CalculateStats(string userId)
        {
            try
            {
                var totalBookings = await _bookingService.GetBookingCountByStatusAsync(userId, "all");
                var completedBookings = await _bookingService.GetBookingCountByStatusAsync(userId, "Completed");
                var pendingBookings = await _bookingService.GetBookingCountByStatusAsync(userId, "Pending");
                var activeClients = ReceivedBookings.Select(b => b.ClientId).Distinct().Count();

                Stats = new DashboardStats
                {
                    TotalBookings = (int)totalBookings,
                    CompletedBookings = (int)completedBookings,
                    PendingBookings = (int)pendingBookings,
                    ActiveClients = activeClients,
                    AverageRating = ProfessionalProfile?.RatingAverage ?? 0,
                    TotalReviews = MyReviews.Count
                };
            }
            catch
            {
                Stats = new DashboardStats();
            }
        }
    }

    public class DashboardStats
    {
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ActiveClients { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
} 