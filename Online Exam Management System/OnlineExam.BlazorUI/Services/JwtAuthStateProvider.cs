using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using OnlineExam.Shared;

namespace OnlineExam.BlazorUI.Services;

public sealed class JwtAuthStateProvider(LocalStorageService storage) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(Anonymous);
        }

        var claims = ParseClaims(token).ToList();
        var exp = claims.FirstOrDefault(x => x.Type == "exp")?.Value;
        if (long.TryParse(exp, out var seconds) &&
            DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime <= DateTime.UtcNow)
        {
            await LogoutAsync();
            return new AuthenticationState(Anonymous);
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public ValueTask<string?> GetTokenAsync() => storage.GetAsync<string>("exam-token");

    public async Task SetLoginAsync(AuthResponse auth)
    {
        await storage.SetAsync("exam-token", auth.Token);
        await storage.SetAsync("exam-user", auth);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        await storage.RemoveAsync("exam-token");
        await storage.RemoveAsync("exam-user");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static IEnumerable<Claim> ParseClaims(string token)
    {
        var payload = token.Split('.')[1];
        var jsonBytes = Convert.FromBase64String(Pad(payload.Replace('-', '+').Replace('_', '/')));
        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes) ?? [];

        foreach (var (key, value) in values)
        {
            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                {
                    yield return new Claim(MapClaimType(key), item.ToString());
                }
            }
            else
            {
                yield return new Claim(MapClaimType(key), value.ToString());
            }
        }
    }

    private static string Pad(string value) => value.PadRight(value.Length + (4 - value.Length % 4) % 4, '=');

    private static string MapClaimType(string type) => type switch
    {
        "role" => ClaimTypes.Role,
        "name" => ClaimTypes.Name,
        "nameid" => ClaimTypes.NameIdentifier,
        "sub" => ClaimTypes.NameIdentifier,
        _ => type
    };
}
