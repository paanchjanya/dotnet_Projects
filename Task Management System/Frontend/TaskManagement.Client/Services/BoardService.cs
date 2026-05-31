using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace TaskManagement.Client.Services;

public enum TaskPriority { Low, Medium, High }

public class ClientUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class ClientBoard
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public ClientUser? Owner { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ClientColumn> Columns { get; set; } = new();
}

public class ClientColumn
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; }
    public Guid BoardId { get; set; }
    public List<ClientTask> Tasks { get; set; } = new();
}

public class ClientTask
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Position { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid ColumnId { get; set; }
    public ClientColumn? Column { get; set; }
    public string? AssignedToUserId { get; set; }
    public ClientUser? AssignedToUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ClientComment> Comments { get; set; } = new();
    public List<ClientAttachment> Attachments { get; set; } = new();
}

public class ClientComment
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid TaskItemId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ClientUser? User { get; set; }
}

public class ClientAttachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public Guid TaskItemId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ClientUser? User { get; set; }
}

public class ClientActivityLog
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ClientUser? User { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class BoardService
{
    private readonly HttpClient _http;
    private readonly string _apiBase = "api";

    public BoardService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClientBoard>> GetBoardsAsync()
    {
        return await _http.GetFromJsonAsync<List<ClientBoard>>($"{_apiBase}/boards") ?? new();
    }

    public async Task<ClientBoard?> GetBoardAsync(Guid id)
    {
        var response = await _http.GetAsync($"{_apiBase}/boards/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ClientBoard>();
        }
        return null;
    }

    public async Task<ClientBoard?> CreateBoardAsync(string name, string description)
    {
        var response = await _http.PostAsJsonAsync($"{_apiBase}/boards", new { name, description });
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ClientBoard>();
        }
        return null;
    }

    public async Task<bool> UpdateBoardAsync(Guid id, string name, string description)
    {
        var response = await _http.PutAsJsonAsync($"{_apiBase}/boards/{id}", new { name, description });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBoardAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{_apiBase}/boards/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ClientActivityLog>> GetActivityLogsAsync(Guid boardId)
    {
        return await _http.GetFromJsonAsync<List<ClientActivityLog>>($"{_apiBase}/boards/{boardId}/activity") ?? new();
    }

    // Columns
    public async Task<ClientColumn?> CreateColumnAsync(Guid boardId, string name)
    {
        var response = await _http.PostAsJsonAsync($"{_apiBase}/columns", new { boardId, name });
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ClientColumn>();
        }
        return null;
    }

    public async Task<bool> UpdateColumnAsync(Guid id, string name)
    {
        var response = await _http.PutAsJsonAsync($"{_apiBase}/columns/{id}", new { name });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteColumnAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{_apiBase}/columns/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReorderColumnsAsync(Guid boardId, List<ColumnOrderDto> columnOrders)
    {
        var response = await _http.PostAsJsonAsync($"{_apiBase}/columns/reorder", new { boardId, columnOrders });
        return response.IsSuccessStatusCode;
    }

    // Tasks
    public async Task<ClientTask?> GetTaskAsync(Guid id)
    {
        var response = await _http.GetAsync($"{_apiBase}/tasks/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ClientTask>();
        }
        return null;
    }

    public async Task<ClientTask?> CreateTaskAsync(Guid columnId, string title, string description, TaskPriority priority, DateTime? dueDate, string? assignedToUserId)
    {
        var response = await _http.PostAsJsonAsync($"{_apiBase}/tasks", new
        {
            columnId,
            title,
            description,
            priority = (int)priority,
            dueDate,
            assignedToUserId
        });
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ClientTask>();
        }
        return null;
    }

    public async Task<bool> UpdateTaskAsync(Guid id, string title, string description, TaskPriority priority, DateTime? dueDate, string? assignedToUserId)
    {
        var response = await _http.PutAsJsonAsync($"{_apiBase}/tasks/{id}", new
        {
            title,
            description,
            priority = (int)priority,
            dueDate,
            assignedToUserId
        });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{_apiBase}/tasks/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MoveTaskAsync(Guid taskId, Guid targetColumnId, int newPosition)
    {
        var response = await _http.PostAsJsonAsync($"{_apiBase}/tasks/move", new { taskId, targetColumnId, newPosition });
        return response.IsSuccessStatusCode;
    }

    // Comments
    public async Task<ClientComment?> AddCommentAsync(Guid taskItemId, string content)
    {
        var response = await _http.PostAsJsonAsync($"{_apiBase}/comments", new { taskItemId, content });
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ClientComment>();
        }
        return null;
    }

    public async Task<bool> DeleteCommentAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{_apiBase}/comments/{id}");
        return response.IsSuccessStatusCode;
    }

    // Attachments
    public async Task<bool> DeleteAttachmentAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{_apiBase}/attachments/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ClientUser>> GetUsersAsync()
    {
        return await _http.GetFromJsonAsync<List<ClientUser>>($"{_apiBase}/auth/users") ?? new();
    }
}

public class ColumnOrderDto
{
    public Guid ColumnId { get; set; }
    public int Position { get; set; }
}
