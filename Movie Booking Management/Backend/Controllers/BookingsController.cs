using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CineBooking.Api.Data;
using CineBooking.Api.DTOs;
using CineBooking.Api.Models;

namespace CineBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> BookSeats([FromBody] BookingRequestDto request)
        {
            if (request == null || request.Seats == null || !request.Seats.Any())
            {
                return BadRequest("Invalid request");
            }

            // Fetch UserId from JWT Claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User not identified");
            }

            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var showtime = await _context.Showtimes.FirstOrDefaultAsync(s => s.Id == request.ShowtimeId);
                if (showtime == null) return NotFound("Showtime not found");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound("User not found");

                decimal totalCost = showtime.TicketPrice * request.Seats.Count;

                if (user.CreditBalance < totalCost)
                {
                    throw new InvalidOperationException("Insufficient balance");
                }

                // Check for existing seats to throw concurrency error manually if needed
                // But the DB composite key will also enforce this. We use Serializable to lock the read/write range.
                var requestedSeats = request.Seats.Select(s => new { s.RowNumber, s.SeatNumber }).ToList();
                var existingTickets = await _context.TicketDetails
                    .Where(t => t.ShowtimeId == request.ShowtimeId)
                    .ToListAsync();
                
                foreach (var existing in existingTickets)
                {
                    if (requestedSeats.Any(s => s.RowNumber == existing.RowNumber && s.SeatNumber == existing.SeatNumber))
                    {
                        throw new InvalidOperationException("Seat Already Reserved");
                    }
                }

                // Deduct balance
                user.CreditBalance -= totalCost;

                var booking = new Booking
                {
                    ShowtimeId = request.ShowtimeId,
                    UserId = user.Id,
                    BookingReferenceNumber = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                    BookingDate = DateTime.UtcNow,
                    TotalAmount = totalCost
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                foreach (var seat in request.Seats)
                {
                    var ticketDetail = new TicketDetail
                    {
                        BookingId = booking.Id,
                        ShowtimeId = request.ShowtimeId,
                        RowNumber = seat.RowNumber,
                        SeatNumber = seat.SeatNumber
                    };
                    _context.TicketDetails.Add(ticketDetail);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { Message = "Booking successful", BookingReference = booking.BookingReferenceNumber });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpGet("{reference}")]
        [Authorize]
        public async Task<IActionResult> GetTicket(string reference)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var booking = await _context.Bookings
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.TicketDetails)
                .FirstOrDefaultAsync(b => b.BookingReferenceNumber == reference && b.UserId == userId);

            if (booking == null) return NotFound("Ticket not found");

            return Ok(new
            {
                MovieTitle = booking.Showtime.Movie.Title,
                StartTime = booking.Showtime.StartTime,
                AmountPaid = booking.TotalAmount,
                BookingReference = booking.BookingReferenceNumber,
                Seats = booking.TicketDetails.Select(t => new { t.RowNumber, t.SeatNumber })
            });
        }

        // GET /api/bookings/seats/{showtimeId} — Return all booked seat IDs for a showtime
        [HttpGet("seats/{showtimeId}")]
        public async Task<IActionResult> GetBookedSeats(int showtimeId)
        {
            var bookedSeats = await _context.TicketDetails
                .Where(t => t.ShowtimeId == showtimeId)
                .Select(t => t.RowNumber + t.SeatNumber)  // e.g. "D2", "D3"
                .ToListAsync();

            return Ok(bookedSeats);
        }
    }
}
