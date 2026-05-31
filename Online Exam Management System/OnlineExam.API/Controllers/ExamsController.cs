using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OnlineExam.Data;
using OnlineExam.Shared;

namespace OnlineExam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ExamsController(ApplicationDbContext db, IDistributedCache cache) : ControllerBase
{
    [HttpGet("active")]
    public async Task<ActionResult<List<ExamDto>>> GetActive()
    {
        var exams = await db.Exams
            .Where(x => x.IsActive)
            .OrderBy(x => x.Title)
            .Select(x => new ExamDto(x.Id, x.Title, x.Description, x.DurationMinutes, x.TotalMarks, x.IsActive))
            .ToListAsync();

        return exams;
    }

    [HttpGet("{examId:int}/questions")]
    public async Task<ActionResult<List<QuestionDto>>> GetQuestions(int examId, [FromQuery] bool includeAnswers = false)
    {
        var key = $"exam:{examId}:questions:{includeAnswers}";
        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<List<QuestionDto>>(cached) ?? [];
        }

        var questions = await db.Questions
            .AsNoTracking()
            .Include(x => x.Options)
            .Where(x => x.ExamId == examId)
            .OrderBy(x => x.OrderNumber)
            .Select(x => new QuestionDto(
                x.Id,
                x.ExamId,
                x.Text,
                x.Type,
                x.Marks,
                x.OrderNumber,
                x.Topic,
                x.Options.Select(o => new OptionDto(o.Id, o.Text, includeAnswers ? o.IsCorrect : null)).ToList()))
            .ToListAsync();

        await cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(questions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) });

        return questions;
    }
}
