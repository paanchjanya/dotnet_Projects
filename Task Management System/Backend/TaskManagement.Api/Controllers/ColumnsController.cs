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
public class ColumnsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<BoardHub> _hubContext;

    public ColumnsController(ApplicationDbContext context, IHubContext<BoardHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateColumn([FromBody] CreateColumnDto dto)
    {
        var board = await _context.Boards.Include(b => b.Columns).FirstOrDefaultAsync(b => b.Id == dto.BoardId);
        if (board == null)
            return NotFound(new { Message = "Board not found." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var position = board.Columns.Any() ? board.Columns.Max(c => c.Position) + 1 : 0;

        var column = new Column
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Position = position,
            BoardId = dto.BoardId
        };

        _context.Columns.Add(column);

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            UserId = userId,
            Message = $"added column '{column.Name}' to the board",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{board.Id}").SendAsync("BoardUpdated", board.Id);

        return Ok(column);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateColumn(Guid id, [FromBody] UpdateColumnDto dto)
    {
        var column = await _context.Columns.FindAsync(id);
        if (column == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var oldName = column.Name;
        column.Name = dto.Name;

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = column.BoardId,
            UserId = userId,
            Message = $"renamed column '{oldName}' to '{column.Name}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{column.BoardId}").SendAsync("BoardUpdated", column.BoardId);

        return Ok(column);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteColumn(Guid id)
    {
        var column = await _context.Columns.FindAsync(id);
        if (column == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var boardId = column.BoardId;

        _context.Columns.Remove(column);

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            UserId = userId,
            Message = $"deleted column '{column.Name}'",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{boardId}").SendAsync("BoardUpdated", boardId);

        return Ok(new { Message = "Column deleted successfully." });
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> ReorderColumns([FromBody] ReorderColumnsDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var columns = await _context.Columns
            .Where(c => c.BoardId == dto.BoardId)
            .ToListAsync();

        foreach (var colOrder in dto.ColumnOrders)
        {
            var column = columns.FirstOrDefault(c => c.Id == colOrder.ColumnId);
            if (column != null)
            {
                column.Position = colOrder.Position;
            }
        }

        _context.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            BoardId = dto.BoardId,
            UserId = userId,
            Message = "reordered columns",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group($"board_{dto.BoardId}").SendAsync("BoardUpdated", dto.BoardId);

        return Ok(new { Message = "Columns reordered." });
    }
}

public class CreateColumnDto
{
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UpdateColumnDto
{
    public string Name { get; set; } = string.Empty;
}

public class ReorderColumnsDto
{
    public Guid BoardId { get; set; }
    public List<ColumnOrderDto> ColumnOrders { get; set; } = new();
}

public class ColumnOrderDto
{
    public Guid ColumnId { get; set; }
    public int Position { get; set; }
}
