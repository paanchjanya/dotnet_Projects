using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CineBooking.Api.DTOs
{
    public class BookingRequestDto
    {
        [Required]
        public int ShowtimeId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one seat must be selected")]
        public List<SeatDto> Seats { get; set; }
    }
}
