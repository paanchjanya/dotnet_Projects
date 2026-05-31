using System;
using System.Collections.Generic;

namespace TaskManagement.Api.Models;

public class Column
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; }
    
    public Guid BoardId { get; set; }
    public Board? Board { get; set; }
    
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
