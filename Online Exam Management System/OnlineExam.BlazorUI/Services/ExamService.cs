using System.Net.Http.Json;
using OnlineExam.Shared;

namespace OnlineExam.BlazorUI.Services;

public sealed class ExamService(HttpClient http) : IExamService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request) =>
        await PostAsync<AuthResponse>("api/auth/login", request);

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request) =>
        await PostAsync<AuthResponse>("api/auth/register", request);

    public async Task<List<ExamDto>> GetActiveExamsAsync() =>
        await http.GetFromJsonAsync<List<ExamDto>>("api/exams/active") ?? [];

    public async Task<List<QuestionDto>> GetQuestionsAsync(int examId) =>
        await http.GetFromJsonAsync<List<QuestionDto>>($"api/exams/{examId}/questions") ?? [];

    public async Task<AttemptResponseDto> StartAttemptAsync(int examId) =>
        await PostAsync<AttemptResponseDto>("api/attempts/start", new StartAttemptRequest(examId));

    public async Task<AnswerSaveResultDto> SaveAnswerAsync(int attemptId, int questionId, int optionId)
    {
        var response = await http.PutAsJsonAsync($"api/attempts/{attemptId}/answer", new AnswerSubmitDto(questionId, optionId));
        return await ReadAsync<AnswerSaveResultDto>(response);
    }

    public async Task<SubmitAttemptResponseDto> SubmitAsync(int attemptId) =>
        await PostAsync<SubmitAttemptResponseDto>($"api/attempts/{attemptId}/submit", new { });

    public async Task<ResultDto> GetResultAsync(int attemptId) =>
        await http.GetFromJsonAsync<ResultDto>($"api/results/{attemptId}")
        ?? throw new InvalidOperationException("Result was empty.");

    public async Task CreateQuestionAsync(QuestionUpsertDto question) =>
        await ReadAsync<QuestionDto>(await http.PostAsJsonAsync("api/admin/questions", question));

    public async Task UpdateQuestionAsync(int id, QuestionUpsertDto question) =>
        await ReadAsync<QuestionDto>(await http.PutAsJsonAsync($"api/admin/questions/{id}", question));

    private async Task<T> PostAsync<T>(string url, object body) =>
        await ReadAsync<T>(await http.PostAsJsonAsync(url, body));

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? response.ReasonPhrase : message);
        }

        return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("Empty response.");
    }
}
