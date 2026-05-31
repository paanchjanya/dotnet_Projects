using Microsoft.EntityFrameworkCore;
using OnlineExam.Shared;

namespace OnlineExam.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(ApplicationDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new AppUser
                {
                    Email = "admin@exam.com",
                    FullName = "Exam Admin",
                    PasswordHash = PasswordHasher.Hash("Admin123!"),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                },
                new AppUser
                {
                    Email = "student@exam.com",
                    FullName = "Demo Student",
                    PasswordHash = PasswordHasher.Hash("Student123!"),
                    Role = UserRole.Student,
                    CreatedAt = DateTime.UtcNow
                });
        }

        if (await db.Exams.AnyAsync())
        {
            await db.SaveChangesAsync();
            return;
        }

        var exam = new Exam
        {
            Title = "C# Fundamentals Sprint",
            Description = "A bright, fast-paced demo exam covering language basics, async code, and web APIs.",
            DurationMinutes = 15,
            TotalMarks = 25,
            IsActive = true,
            Questions =
            [
                Question("Which keyword defines an asynchronous method?", "C# Basics", 1, ["async", "await", "yield", "lock"], 0),
                Question("A Blazor WebAssembly app runs primarily in the browser.", "Blazor", 2, ["True", "False"], 0, QuestionType.TrueFalse),
                Question("Which HTTP status code usually means unauthorized?", "Web APIs", 3, ["200", "201", "401", "500"], 2),
                Question("What does EF Core primarily provide?", "Data", 4, ["Object-relational mapping", "CSS bundling", "Image compression", "Email hosting"], 0),
                Question("Which service pushes real-time updates in ASP.NET Core?", "Realtime", 5, ["SignalR", "Razor", "LocalDB", "LINQ"], 0)
            ]
        };

        db.Exams.Add(exam);
        await db.SaveChangesAsync();
    }

    private static Question Question(
        string text,
        string topic,
        int order,
        IReadOnlyList<string> options,
        int correctIndex,
        QuestionType type = QuestionType.MultipleChoice) =>
        new()
        {
            Text = text,
            Topic = topic,
            Type = type,
            Marks = 5,
            OrderNumber = order,
            Options = options.Select((option, index) => new Option { Text = option, IsCorrect = index == correctIndex }).ToList()
        };
}
