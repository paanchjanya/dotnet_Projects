using System;

namespace TaskManagement.Api.Models;

public class Attachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
