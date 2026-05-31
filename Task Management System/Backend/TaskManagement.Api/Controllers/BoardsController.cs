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
public class BoardsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<BoardHub> _hubContext;

    public BoardsController(ApplicationDbContext context, IHubContext<BoardHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetBoards()
    {
        var boards = await _context.Boards
            .Include(b => b.Owner)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return Ok(boards);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBoard(Guid id)
    {
        var board = await _context.Boards
            .Include(b => b.Owner)
            .Include(b => b.Columns.OrderBy(c => c.Position))
                .ThenInclude(c => c.Tasks.OrderBy(t => t.Position))
                    .ThenInclude(t => t.AssignedToUser)
            .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
                    .ThenInclude(t => t.Comments)
            .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
                    .ThenInclude(t => t.Attachments)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (board == null)
            return NotFound(new { Message = "Board not found." });

        return Ok(board);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        // Create default columns
        board.Columns.Add(new Column { Id = Guid.NewGuid(), Name = "To Do", Position = 0 });
        board.Columns.Add(new Column { Id = Guid.NewGuid(), Name = "In Progress", Position = 1 });
        board.Columns.Add(new Column { Id = Guid.NewGuid(), Name = "Done", Position = 2 });

        _context.Boards.Add(board);

        // Add activity log
        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            UserId = userId,
            Message = $"created the board '{board.Name}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("BoardListUpdated");

        return CreatedAtAction(nameof(GetBoard), new { id = board.Id }, board);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardDto dto)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        board.Name = dto.Name;
        board.Description = dto.Description;

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            UserId = userId,
            Message = $"updated board details to '{board.Name}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{id}").SendAsync("BoardUpdated", id);
        await _hubContext.Clients.All.SendAsync("BoardListUpdated");

        return Ok(board);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(Guid id)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board == null)
            return NotFound();

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{id}").SendAsync("BoardDeleted", id);
        await _hubContext.Clients.All.SendAsync("BoardListUpdated");

        return Ok(new { Message = "Board deleted successfully." });
    }

    [HttpGet("{id}/activity")]
    public async Task<IActionResult> GetActivity(Guid id)
    {
        var logs = await _context.ActivityLogs
            .Include(a => a.User)
            .Where(a => a.BoardId == id)
            .OrderByDescending(a => a.CreatedAt)
            .Take(30)
            .ToListAsync();
        return Ok(logs);
    }
}

public class CreateBoardDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateBoardDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
