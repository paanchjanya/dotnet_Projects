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
public class AttachmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IHubContext<BoardHub> _hubContext;

    public AttachmentsController(ApplicationDbContext context, IWebHostEnvironment env, IHubContext<BoardHub> hubContext)
    {
        _context = context;
        _env = env;
        _hubContext = hubContext;
    }

    [HttpPost("upload")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadAttachment([FromForm] Guid taskItemId, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var task = await _context.Tasks
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == taskItemId);

        if (task == null)
            return NotFound("Task not found.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Ensure wwwroot/uploads exists
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }
        var uploadsDir = Path.Combine(webRoot, "uploads");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/{uniqueFileName}";

        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            FilePath = relativePath,
            TaskItemId = taskItemId,
            UserId = userId,
            UploadedAt = DateTime.UtcNow
        };

        _context.Attachments.Add(attachment);

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = task.Column!.BoardId,
            UserId = userId,
            Message = $"attached file '{file.FileName}' to task '{task.Title}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{task.Column.BoardId}").SendAsync("BoardUpdated", task.Column.BoardId);

        return Ok(attachment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttachment(Guid id)
    {
        var attachment = await _context.Attachments
            .Include(a => a.TaskItem)
                .ThenInclude(t => t!.Column)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var boardId = attachment.TaskItem!.Column!.BoardId;

        // Delete physical file
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }
        var physicalPath = Path.Combine(webRoot, attachment.FilePath.TrimStart('/'));
        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }

        _context.Attachments.Remove(attachment);

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            UserId = userId,
            Message = $"removed file attachment '{attachment.FileName}' from task '{attachment.TaskItem.Title}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{boardId}").SendAsync("BoardUpdated", boardId);

        return Ok(new { Message = "Attachment deleted." });
    }
}
