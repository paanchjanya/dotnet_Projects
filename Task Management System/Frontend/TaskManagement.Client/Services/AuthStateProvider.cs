using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

namespace TaskManagement.Client.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorage;
    private readonly AuthenticationState _anonymous;

    public AuthStateProvider(HttpClient httpClient, LocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                return _anonymous;
            }

            var claims = ParseClaimsFromJwt(token).ToList();
            var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim != null && long.TryParse(expClaim.Value, out var expSecs))
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(expSecs).UtcDateTime;
                if (expTime <= DateTime.UtcNow)
                {
                    await MarkUserAsLoggedOut();
                    return _anonymous;
                }
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }
        catch
        {
            return _anonymous;
        }
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorage.SetItemAsync("authToken", token);
        var claims = ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        var authState = Task.FromResult(_anonymous);
        NotifyAuthenticationStateChanged(authState);
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        
        var claims = new List<Claim>();
        if (keyValuePairs == null) return claims;

        if (keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles))
        {
            if (roles.ToString()!.Trim().StartsWith("["))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);
                if (parsedRoles != null)
                {
                    claims.AddRange(parsedRoles.Select(role => new Claim(ClaimTypes.Role, role)));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
            }
            keyValuePairs.Remove(ClaimTypes.Role);
        }

        foreach (var kvp in keyValuePairs)
        {
            var valueStr = kvp.Value?.ToString() ?? "";
            
            var type = kvp.Key switch
            {
                "unique_name" => ClaimTypes.Name,
                "name" => ClaimTypes.Name,
                "nameid" => ClaimTypes.NameIdentifier,
                "sub" => ClaimTypes.NameIdentifier,
                "email" => ClaimTypes.Email,
                _ => kvp.Key
            };

            claims.Add(new Claim(type, valueStr));
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
