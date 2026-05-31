using System.Text.Json.Serialization;

namespace EmployeeLeaveApi.Models
{
    public class LeaveBalance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal UsedDays { get; set; } = 0;
        public decimal PendingDays { get; set; } = 0;

        // Navigation properties
        [JsonIgnore]
        public User? Employee { get; set; }
        public LeaveType? LeaveType { get; set; }
    }
}
