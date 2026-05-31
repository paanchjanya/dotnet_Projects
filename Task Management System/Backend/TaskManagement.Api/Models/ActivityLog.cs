using System;

namespace TaskManagement.Api.Models;

public class ActivityLog
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
