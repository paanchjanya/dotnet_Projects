using System;

namespace TaskManagement.Api.Models;

public class Comment
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
