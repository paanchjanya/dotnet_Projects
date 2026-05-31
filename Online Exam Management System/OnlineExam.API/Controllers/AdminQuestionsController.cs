using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OnlineExam.Data;
using OnlineExam.Shared;

namespace OnlineExam.API.Controllers;

[ApiController]
[Route("api/admin/questions")]
[Authorize(Roles = "Admin")]
public sealed class AdminQuestionsController(ApplicationDbContext db, IDistributedCache cache) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<QuestionDto>> Create(QuestionUpsertDto request)
    {
        var validation = ValidateQuestion(request);
        if (validation is not null)
        {
            return BadRequest(validation);
        }

        var question = new Question
        {
            ExamId = request.ExamId,
            Text = request.Text,
            Type = request.Type,
            Marks = request.Marks,
            OrderNumber = request.OrderNumber,
            Topic = request.Topic,
            Options = request.Options.Select(x => new Option { Text = x.Text, IsCorrect = x.IsCorrect }).ToList()
        };

        db.Questions.Add(question);
        await db.SaveChangesAsync();
        await Invalidate(question.ExamId);

        return CreatedAtAction(nameof(Create), ToDto(question));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<QuestionDto>> Update(int id, QuestionUpsertDto request)
    {
        var validation = ValidateQuestion(request);
        if (validation is not null)
        {
            return BadRequest(validation);
        }

        var question = await db.Questions.Include(x => x.Options).SingleOrDefaultAsync(x => x.Id == id);
        if (question is null)
        {
            return NotFound();
        }

        question.Text = request.Text;
        question.Type = request.Type;
        question.Marks = request.Marks;
        question.OrderNumber = request.OrderNumber;
        question.Topic = request.Topic;

        db.Options.RemoveRange(question.Options);
        question.Options = request.Options.Select(x => new Option { QuestionId = id, Text = x.Text, IsCorrect = x.IsCorrect }).ToList();

        await db.SaveChangesAsync();
        await Invalidate(question.ExamId);

        return ToDto(question);
    }

    private static string? ValidateQuestion(QuestionUpsertDto request)
    {
        if (request.Options.Count < 2)
        {
            return "At least two options are required.";
        }

        if (request.Options.Count(x => x.IsCorrect) != 1)
        {
            return "Exactly one option must be marked correct.";
        }

        return null;
    }

    private async Task Invalidate(int examId)
    {
        await cache.RemoveAsync($"exam:{examId}:questions:False");
        await cache.RemoveAsync($"exam:{examId}:questions:True");
    }

    private static QuestionDto ToDto(Question question) =>
        new(
            question.Id,
            question.ExamId,
            question.Text,
            question.Type,
            question.Marks,
            question.OrderNumber,
            question.Topic,
            question.Options.Select(x => new OptionDto(x.Id, x.Text, x.IsCorrect)).ToList());
}
