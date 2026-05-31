using OnlineExam.Shared;

namespace OnlineExam.Data;

public sealed class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Attempt> Attempts { get; set; } = [];
}

public sealed class Exam
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int TotalMarks { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Question> Questions { get; set; } = [];
    public List<Attempt> Attempts { get; set; } = [];
}

public sealed class Question
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int Marks { get; set; }
    public int OrderNumber { get; set; }
    public string Topic { get; set; } = "General";
    public List<Option> Options { get; set; } = [];
}

public sealed class Option
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public sealed class Attempt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Score { get; set; }
    public AttemptStatus Status { get; set; }
    public List<UserAnswer> Answers { get; set; } = [];
}

public sealed class UserAnswer
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public Attempt? Attempt { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    public int? SelectedOptionId { get; set; }
    public Option? SelectedOption { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
}
