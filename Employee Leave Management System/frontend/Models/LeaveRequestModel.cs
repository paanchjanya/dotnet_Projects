using System;

namespace EmployeeLeaveClient.Models
{
    public class LeaveRequestModel
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
        public DateTime AppliedDate { get; set; }
        public string? ManagerRemarks { get; set; }
        public DateTime? ActionedDate { get; set; }
        public int? ApprovedByManagerId { get; set; }

        public UserModel? Employee { get; set; }
        public LeaveTypeModel? LeaveType { get; set; }
    }
}
