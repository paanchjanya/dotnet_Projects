using System;

namespace EmployeeLeaveClient.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee"; // Admin, Manager, Employee
        public int? ManagerId { get; set; }
        public DateTime JoinDate { get; set; }
        public string? ProfilePictureUrl { get; set; }
        
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
