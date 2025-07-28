using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProConnect.Application.DTOs;
using ProConnect.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProConnect.Pages.Bookings
{
    [Authorize]
    public class BookingsDetailsModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public BookingDto? Booking { get; set; }

        public BookingsDetailsModel(IBookingService bookingService) : base()
        {
            _bookingService = bookingService;
        }

        public async Task<IActionResult> OnGetAsync(string bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(bookingId))
            {
                Booking = null;
                return Page();
            }
            // Solo permite ver la reserva si pertenece al usuario
            var booking = await _bookingService.GetBookingByIdAsync(bookingId, userId);
            Booking = booking;
            return Page();
        }

        public bool CanCancelBooking()
        {
            if (Booking == null) return false;
            // Solo se puede cancelar si está pendiente o confirmada y faltan más de 24h
            if (Booking.Status != "Pending" && Booking.Status != "Confirmed")
                return false;
            var hoursUntilAppointment = (Booking.AppointmentDate - System.DateTime.UtcNow).TotalHours;
            return hoursUntilAppointment > 24;
        }

        public async Task<IActionResult> OnPostCancelAsync(string bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(bookingId))
            {
                return RedirectToPage("/bookings");
            }
            // Lógica de cancelación (puedes implementar _bookingService.CancelBookingAsync)
            // await _bookingService.CancelBookingAsync(bookingId, userId);
            return RedirectToPage("/bookings");
        }
    }
}
