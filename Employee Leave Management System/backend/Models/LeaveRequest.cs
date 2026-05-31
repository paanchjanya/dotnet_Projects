using System;
using System.Text.Json.Serialization;

namespace EmployeeLeaveApi.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled
        public string? AttachmentPath { get; set; }
        public string? AttachmentFileName { get; set; }
        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
        public string? ManagerRemarks { get; set; }
        public DateTime? ActionedDate { get; set; }
        public int? ApprovedByManagerId { get; set; }

        // Navigation properties
        public User? Employee { get; set; }
        public LeaveType? LeaveType { get; set; }
        
        [JsonIgnore]
        public User? ApprovedByManager { get; set; }
    }
}
