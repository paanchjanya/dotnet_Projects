using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineExam.API.Hubs;
using OnlineExam.API.Services;
using OnlineExam.Data;
using OnlineExam.Shared;

namespace OnlineExam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AttemptsController(
    ApplicationDbContext db,
    CurrentUserService currentUser,
    IHubContext<LeaderboardHub> hub) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<AttemptResponseDto>> Start(StartAttemptRequest request)
    {
        var exam = await db.Exams.FindAsync(request.ExamId);
        if (exam is null || !exam.IsActive)
        {
            return NotFound("Exam not found or inactive.");
        }

        var now = DateTime.UtcNow;
        var attempt = await db.Attempts
            .FirstOrDefaultAsync(x => x.ExamId == exam.Id && x.UserId == currentUser.UserId && x.Status == AttemptStatus.InProgress);

        if (attempt is not null)
        {
            var expiresAt = attempt.StartTime.AddMinutes(exam.DurationMinutes);
            if (now > expiresAt)
            {
                attempt.EndTime = expiresAt;
                attempt.Status = AttemptStatus.TimedOut;
                attempt.Score = await db.UserAnswers
                    .Where(x => x.AttemptId == attempt.Id)
                    .SumAsync(x => x.PointsEarned);
                
                await db.SaveChangesAsync();
                await BroadcastLeaderboard(exam.Id);
                attempt = null;
            }
        }

        if (attempt is null)
        {
            attempt = new Attempt
            {
                ExamId = exam.Id,
                UserId = currentUser.UserId,
                StartTime = now,
                Status = AttemptStatus.InProgress
            };

            db.Attempts.Add(attempt);
            await db.SaveChangesAsync();
        }

        var selectedAnswers = await db.UserAnswers
            .Where(x => x.AttemptId == attempt.Id && x.SelectedOptionId.HasValue)
            .ToDictionaryAsync(x => x.QuestionId, x => x.SelectedOptionId!.Value);

        return new AttemptResponseDto(
            attempt.Id,
            exam.Id,
            attempt.StartTime,
            attempt.StartTime.AddMinutes(exam.DurationMinutes),
            attempt.Status,
            selectedAnswers);
    }

    [HttpPut("{attemptId:int}/answer")]
    public async Task<ActionResult<AnswerSaveResultDto>> SaveAnswer(int attemptId, AnswerSubmitDto request)
    {
        var attempt = await db.Attempts
            .Include(x => x.Exam)
            .SingleOrDefaultAsync(x => x.Id == attemptId && x.UserId == currentUser.UserId);

        if (attempt is null)
        {
            return NotFound();
        }

        if (attempt.Status != AttemptStatus.InProgress)
        {
            return BadRequest("Attempt is already closed.");
        }

        var question = await db.Questions.Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == request.QuestionId && x.ExamId == attempt.ExamId);
        var selected = question?.Options.SingleOrDefault(x => x.Id == request.SelectedOptionId);

        if (question is null || selected is null)
        {
            return BadRequest("Invalid question or option.");
        }

        var answer = await db.UserAnswers
            .SingleOrDefaultAsync(x => x.AttemptId == attemptId && x.QuestionId == request.QuestionId);

        var isCorrect = selected.IsCorrect;
        var points = isCorrect ? question.Marks : 0;

        if (answer is null)
        {
            answer = new UserAnswer { AttemptId = attemptId, QuestionId = question.Id };
            db.UserAnswers.Add(answer);
        }

        answer.SelectedOptionId = selected.Id;
        answer.IsCorrect = isCorrect;
        answer.PointsEarned = points;

        await db.SaveChangesAsync();
        attempt.Score = await db.UserAnswers.Where(x => x.AttemptId == attemptId).SumAsync(x => x.PointsEarned);
        await db.SaveChangesAsync();

        await BroadcastLeaderboard(attempt.ExamId);

        return new AnswerSaveResultDto(question.Id, selected.Id, isCorrect, points, attempt.Score);
    }

    [HttpPost("{attemptId:int}/submit")]
    public async Task<ActionResult<SubmitAttemptResponseDto>> Submit(int attemptId)
    {
        var attempt = await db.Attempts
            .Include(x => x.Exam)
            .SingleOrDefaultAsync(x => x.Id == attemptId && x.UserId == currentUser.UserId);

        if (attempt is null || attempt.Exam is null)
        {
            return NotFound();
        }

        attempt.Score = await db.UserAnswers.Where(x => x.AttemptId == attemptId).SumAsync(x => x.PointsEarned);
        attempt.EndTime = DateTime.UtcNow;
        attempt.Status = DateTime.UtcNow > attempt.StartTime.AddMinutes(attempt.Exam.DurationMinutes)
            ? AttemptStatus.TimedOut
            : AttemptStatus.Completed;

        await db.SaveChangesAsync();
        await BroadcastLeaderboard(attempt.ExamId);

        var percentage = attempt.Exam.TotalMarks == 0 ? 0 : Math.Round(attempt.Score * 100d / attempt.Exam.TotalMarks, 2);
        return new SubmitAttemptResponseDto(attempt.Id, attempt.Score, attempt.Exam.TotalMarks, percentage, attempt.Status);
    }

    private async Task BroadcastLeaderboard(int examId)
    {
        var examTotal = await db.Exams.Where(x => x.Id == examId).Select(x => x.TotalMarks).SingleAsync();
        var leaders = await db.Attempts
            .Include(x => x.User)
            .Where(x => x.ExamId == examId)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.EndTime ?? DateTime.MaxValue)
            .Take(10)
            .Select(x => new LeaderboardEntryDto(
                x.User!.FullName,
                x.Score,
                examTotal,
                examTotal == 0 ? 0 : Math.Round(x.Score * 100d / examTotal, 2)))
            .ToListAsync();

        await hub.Clients.Group(LeaderboardHub.GroupName(examId)).SendAsync("LeaderboardUpdated", leaders);
    }
}
