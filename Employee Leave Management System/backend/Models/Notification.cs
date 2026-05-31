using System;
using System.Text.Json.Serialization;

namespace EmployeeLeaveApi.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [JsonIgnore]
        public User? User { get; set; }
    }
}
