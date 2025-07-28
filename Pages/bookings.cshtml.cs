using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;

namespace ProConnect.Pages
{
    [Authorize]
    public class BookingsModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public List<BookingDto> Bookings { get; set; } = new List<BookingDto>();

        public BookingsModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Bookings = new List<BookingDto>();
                return;
            }
            // Obtener todas las citas del usuario autenticado (como cliente)
            var bookings = await _bookingService.GetBookingsByClientAsync(userId, 100, 0);
            Bookings = bookings ?? new List<BookingDto>();
        }
    }
}
