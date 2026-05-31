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
public class CommentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<BoardHub> _hubContext;

    public CommentsController(ApplicationDbContext context, IHubContext<BoardHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == dto.TaskItemId);

        if (task == null)
            return NotFound(new { Message = "Task not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = dto.Content,
            TaskItemId = dto.TaskItemId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = task.Column!.BoardId,
            UserId = userId,
            Message = $"commented on task '{task.Title}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{task.Column.BoardId}").SendAsync("BoardUpdated", task.Column.BoardId);

        return Ok(comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var comment = await _context.Comments
            .Include(c => c.TaskItem)
                .ThenInclude(t => t!.Column)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Only comment author or Admin can delete
        var isAdmin = User.IsInRole("Admin");
        if (comment.UserId != userId && !isAdmin)
            return Forbid();

        var boardId = comment.TaskItem!.Column!.BoardId;
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{boardId}").SendAsync("BoardUpdated", boardId);

        return Ok(new { Message = "Comment deleted." });
    }
}

public class AddCommentDto
{
    public Guid TaskItemId { get; set; }
    public string Content { get; set; } = string.Empty;
}
