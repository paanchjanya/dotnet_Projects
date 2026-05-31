using Microsoft.AspNetCore.SignalR;

namespace OnlineExam.API.Hubs;

public sealed class LeaderboardHub : Hub
{
    public Task JoinExam(int examId) => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(examId));

    public Task LeaveExam(int examId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(examId));

    public static string GroupName(int examId) => $"exam-{examId}";
}
