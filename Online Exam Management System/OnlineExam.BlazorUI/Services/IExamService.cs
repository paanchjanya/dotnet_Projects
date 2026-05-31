using OnlineExam.Shared;

namespace OnlineExam.BlazorUI.Services;

public interface IExamService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<List<ExamDto>> GetActiveExamsAsync();
    Task<List<QuestionDto>> GetQuestionsAsync(int examId);
    Task<AttemptResponseDto> StartAttemptAsync(int examId);
    Task<AnswerSaveResultDto> SaveAnswerAsync(int attemptId, int questionId, int optionId);
    Task<SubmitAttemptResponseDto> SubmitAsync(int attemptId);
    Task<ResultDto> GetResultAsync(int attemptId);
    Task CreateQuestionAsync(QuestionUpsertDto question);
    Task UpdateQuestionAsync(int id, QuestionUpsertDto question);
}
