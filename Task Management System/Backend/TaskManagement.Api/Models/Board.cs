using System;
using System.Collections.Generic;

namespace TaskManagement.Api.Models;

public class Board
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser? Owner { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Column> Columns { get; set; } = new List<Column>();
}
