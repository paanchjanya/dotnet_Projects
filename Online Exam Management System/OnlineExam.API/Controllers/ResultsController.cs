using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineExam.API.Services;
using OnlineExam.Data;
using OnlineExam.Shared;

namespace OnlineExam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ResultsController(ApplicationDbContext db, CurrentUserService currentUser) : ControllerBase
{
    [HttpGet("{attemptId:int}")]
    public async Task<ActionResult<ResultDto>> Get(int attemptId)
    {
        var attempt = await db.Attempts
            .Include(x => x.Exam)
            .SingleOrDefaultAsync(x => x.Id == attemptId && x.UserId == currentUser.UserId);

        if (attempt?.Exam is null)
        {
            return NotFound();
        }

        var questions = await db.Questions
            .Include(x => x.Options)
            .Where(x => x.ExamId == attempt.ExamId)
            .OrderBy(x => x.OrderNumber)
            .ToListAsync();

        var answers = await db.UserAnswers
            .Include(x => x.SelectedOption)
            .Where(x => x.AttemptId == attemptId)
            .ToDictionaryAsync(x => x.QuestionId);

        var questionResults = questions.Select(question =>
        {
            answers.TryGetValue(question.Id, out var answer);
            var correct = question.Options.Single(x => x.IsCorrect);
            return new QuestionResultDto(
                question.Id,
                question.Text,
                question.Topic,
                question.Marks,
                answer?.SelectedOptionId,
                answer?.SelectedOption?.Text,
                correct.Text,
                answer?.IsCorrect ?? false,
                answer?.PointsEarned ?? 0);
        }).ToList();

        var topicScores = questionResults
            .GroupBy(x => x.Topic)
            .Select(group => new TopicScoreDto(
                group.Key,
                group.Sum(x => x.PointsEarned),
                group.Sum(x => x.Marks),
                group.Sum(x => x.Marks) == 0 ? 0 : Math.Round(group.Sum(x => x.PointsEarned) * 100d / group.Sum(x => x.Marks), 2)))
            .ToList();

        var percentage = attempt.Exam.TotalMarks == 0 ? 0 : Math.Round(attempt.Score * 100d / attempt.Exam.TotalMarks, 2);
        return new ResultDto(
            attempt.Id,
            attempt.Exam.Title,
            attempt.Score,
            attempt.Exam.TotalMarks,
            percentage,
            attempt.Status,
            attempt.StartTime,
            attempt.EndTime,
            questionResults,
            topicScores);
    }
}
