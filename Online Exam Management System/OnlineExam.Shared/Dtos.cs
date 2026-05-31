namespace OnlineExam.Shared;

public sealed record RegisterRequest(string Email, string FullName, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string Token, string Email, string FullName, UserRole Role);

public sealed record ExamDto(
    int Id,
    string Title,
    string Description,
    int DurationMinutes,
    int TotalMarks,
    bool IsActive);

public sealed record OptionDto(int Id, string Text, bool? IsCorrect = null);

public sealed record QuestionDto(
    int Id,
    int ExamId,
    string Text,
    QuestionType Type,
    int Marks,
    int OrderNumber,
    string Topic,
    List<OptionDto> Options);

public sealed record StartAttemptRequest(int ExamId);

public sealed record AttemptResponseDto(
    int AttemptId,
    int ExamId,
    DateTime StartTime,
    DateTime ExpiresAt,
    AttemptStatus Status,
    Dictionary<int, int>? SelectedAnswers = null);

public sealed record AnswerSubmitDto(int QuestionId, int SelectedOptionId);

public sealed record AnswerSaveResultDto(
    int QuestionId,
    int SelectedOptionId,
    bool IsCorrect,
    int PointsEarned,
    int CurrentScore);

public sealed record SubmitAttemptResponseDto(
    int AttemptId,
    int Score,
    int TotalMarks,
    double Percentage,
    AttemptStatus Status);

public sealed record QuestionResultDto(
    int QuestionId,
    string QuestionText,
    string Topic,
    int Marks,
    int? SelectedOptionId,
    string? SelectedOptionText,
    string CorrectOptionText,
    bool IsCorrect,
    int PointsEarned);

public sealed record TopicScoreDto(string Topic, int PointsEarned, int TotalMarks, double AveragePercentage);

public sealed record ResultDto(
    int AttemptId,
    string ExamTitle,
    int Score,
    int TotalMarks,
    double Percentage,
    AttemptStatus Status,
    DateTime StartTime,
    DateTime? EndTime,
    List<QuestionResultDto> Questions,
    List<TopicScoreDto> TopicScores);

public sealed record QuestionUpsertDto(
    int ExamId,
    string Text,
    QuestionType Type,
    int Marks,
    int OrderNumber,
    string Topic,
    List<OptionUpsertDto> Options);

public sealed record OptionUpsertDto(int? Id, string Text, bool IsCorrect);

public sealed record LeaderboardEntryDto(string FullName, int Score, int TotalMarks, double Percentage);
