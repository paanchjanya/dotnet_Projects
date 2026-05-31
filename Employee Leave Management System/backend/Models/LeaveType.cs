namespace EmployeeLeaveApi.Models
{
    public class LeaveType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DefaultDays { get; set; }
        public bool RequiresAttachment { get; set; }
    }
}
