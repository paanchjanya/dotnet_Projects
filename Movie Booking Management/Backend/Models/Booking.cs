using System;
using System.Collections.Generic;

namespace CineBooking.Api.Models
{
    public class Booking
    {
        public int Id { get; set; }
        
        public string BookingReferenceNumber { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; }
        
        public DateTime BookingDate { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public ICollection<TicketDetail> TicketDetails { get; set; }
    }
}
