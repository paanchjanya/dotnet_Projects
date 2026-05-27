using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CineBooking.Api.Models
{
    public class Showtime
    {
        public int Id { get; set; }
        
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public decimal TicketPrice { get; set; }
        
        public ICollection<Booking> Bookings { get; set; }
    }
}
