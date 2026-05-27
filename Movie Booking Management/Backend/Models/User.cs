namespace CineBooking.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // e.g., "Admin", "Customer"
        public decimal CreditBalance { get; set; }
    }
}
