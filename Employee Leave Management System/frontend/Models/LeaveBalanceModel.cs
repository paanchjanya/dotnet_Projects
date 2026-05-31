namespace EmployeeLeaveClient.Models
{
    public class LeaveBalanceModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal PendingDays { get; set; }

        public LeaveTypeModel? LeaveType { get; set; }
        
        public decimal AvailableDays => AllocatedDays - UsedDays - PendingDays;
    }
}
