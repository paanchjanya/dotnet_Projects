namespace CineBooking.Api.Models
{
    public class TicketDetail
    {
        public int Id { get; set; }
        
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        
        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; }
        
        public string RowNumber { get; set; }
        
        public string SeatNumber { get; set; }
    }
}
