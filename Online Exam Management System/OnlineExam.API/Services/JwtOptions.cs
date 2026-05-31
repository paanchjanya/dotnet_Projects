namespace OnlineExam.API.Services;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "OnlineExam.API";
    public string Audience { get; set; } = "OnlineExam.BlazorUI";
    public string SigningKey { get; set; } = "change-this-development-signing-key-with-at-least-32-characters";
    public int ExpiryMinutes { get; set; } = 240;
}
