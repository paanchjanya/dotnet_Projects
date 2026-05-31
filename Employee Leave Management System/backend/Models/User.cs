using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EmployeeLeaveApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee"; // Admin, Manager, Employee
        public int? ManagerId { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public string? ProfilePictureUrl { get; set; }

        // Navigation properties
        [JsonIgnore]
        public User? Manager { get; set; }
        
        [JsonIgnore]
        public ICollection<User> DirectReports { get; set; } = new List<User>();
        
        [JsonIgnore]
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        
        [JsonIgnore]
        public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
        
        [JsonIgnore]
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
