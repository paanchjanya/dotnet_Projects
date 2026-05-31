using System;
using System.Collections.Generic;

namespace TaskManagement.Api.Models;

public enum TaskPriority
{
    Low,
    Medium,
    High
}

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Position { get; set; }
    
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    
    public Guid ColumnId { get; set; }
    public Column? Column { get; set; }
    
    public string? AssignedToUserId { get; set; }
    public ApplicationUser? AssignedToUser { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
