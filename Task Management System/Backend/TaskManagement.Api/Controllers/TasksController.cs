using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Hubs;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<BoardHub> _hubContext;

    public TasksController(ApplicationDbContext context, IHubContext<BoardHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                .ThenInclude(c => c.User)
            .Include(t => t.Attachments.OrderByDescending(a => a.UploadedAt))
                .ThenInclude(a => a.User)
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound(new { Message = "Task not found." });

        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        var column = await _context.Columns
            .Include(c => c.Tasks)
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == dto.ColumnId);

        if (column == null)
            return NotFound(new { Message = "Column not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var position = column.Tasks.Any() ? column.Tasks.Max(t => t.Position) + 1 : 0;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            ColumnId = dto.ColumnId,
            AssignedToUserId = dto.AssignedToUserId,
            Position = position,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = column.BoardId,
            UserId = userId,
            Message = $"created task '{task.Title}' in column '{column.Name}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{column.BoardId}").SendAsync("BoardUpdated", column.BoardId);

        return Ok(task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var oldTitle = task.Title;
        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.AssignedToUserId = dto.AssignedToUserId;

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = task.Column!.BoardId,
            UserId = userId,
            Message = $"updated task '{oldTitle}' properties",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{task.Column.BoardId}").SendAsync("BoardUpdated", task.Column.BoardId);

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var task = await _context.Tasks
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var boardId = task.Column!.BoardId;
        _context.Tasks.Remove(task);

        // Adjust positions of remaining items in the column
        var remaining = await _context.Tasks
            .Where(t => t.ColumnId == task.ColumnId && t.Id != id)
            .OrderBy(t => t.Position)
            .ToListAsync();

        for (int i = 0; i < remaining.Count; i++)
        {
            remaining[i].Position = i;
        }

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            UserId = userId,
            Message = $"deleted task '{task.Title}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{boardId}").SendAsync("BoardUpdated", boardId);

        return Ok(new { Message = "Task deleted." });
    }

    [HttpPost("move")]
    public async Task<IActionResult> MoveTask([FromBody] MoveTaskDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == dto.TaskId);

        if (task == null)
            return NotFound(new { Message = "Task not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var sourceColumnId = task.ColumnId;
        var targetColumnId = dto.TargetColumnId;
        var boardId = task.Column!.BoardId;

        var sourceColumn = await _context.Columns.FindAsync(sourceColumnId);
        var targetColumn = await _context.Columns.FindAsync(targetColumnId);

        if (sourceColumn == null || targetColumn == null)
            return BadRequest("Invalid columns.");

        if (sourceColumn.BoardId != boardId || targetColumn.BoardId != boardId)
            return BadRequest("Columns must belong to the same board.");

        // Fetch source and target tasks
        var sourceTasks = await _context.Tasks
            .Where(t => t.ColumnId == sourceColumnId)
            .OrderBy(t => t.Position)
            .ToListAsync();

        if (sourceColumnId == targetColumnId)
        {
            // Moving within the same column
            sourceTasks.Remove(task);
            var clampedPosition = Math.Max(0, Math.Min(dto.NewPosition, sourceTasks.Count));
            sourceTasks.Insert(clampedPosition, task);

            for (int i = 0; i < sourceTasks.Count; i++)
            {
                sourceTasks[i].Position = i;
            }
        }
        else
        {
            // Moving to a different column
            sourceTasks.Remove(task);
            for (int i = 0; i < sourceTasks.Count; i++)
            {
                sourceTasks[i].Position = i;
            }

            var targetTasks = await _context.Tasks
                .Where(t => t.ColumnId == targetColumnId)
                .OrderBy(t => t.Position)
                .ToListAsync();

            task.ColumnId = targetColumnId;
            var clampedPosition = Math.Max(0, Math.Min(dto.NewPosition, targetTasks.Count));
            targetTasks.Insert(clampedPosition, task);

            for (int i = 0; i < targetTasks.Count; i++)
            {
                targetTasks[i].Position = i;
            }
        }

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            UserId = userId,
            Message = $"moved task '{task.Title}' to '{targetColumn.Name}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{boardId}").SendAsync("BoardUpdated", boardId);

        return Ok(new { Message = "Task moved." });
    }
}

public class CreateTaskDto
{
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedToUserId { get; set; }
}

public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedToUserId { get; set; }
}

public class MoveTaskDto
{
    public Guid TaskId { get; set; }
    public Guid TargetColumnId { get; set; }
    public int NewPosition { get; set; }
}
