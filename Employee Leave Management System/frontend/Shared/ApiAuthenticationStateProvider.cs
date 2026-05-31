using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace EmployeeLeaveClient.Shared
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public ApiAuthenticationStateProvider(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            try
            {
                var claims = ParseClaimsFromJwt(savedToken);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
            }
            catch (Exception)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                }
                catch { }
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void MarkUserAsAuthenticated(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                // Check expiration
                if (keyValuePairs.TryGetValue("exp", out var expObj) && expObj != null)
                {
                    long expSeconds = 0;
                    if (expObj is JsonElement element && element.ValueKind == JsonValueKind.Number)
                    {
                        expSeconds = element.GetInt64();
                    }
                    else if (long.TryParse(expObj.ToString(), out long parsedExp))
                    {
                        expSeconds = parsedExp;
                    }

                    if (expSeconds > 0)
                    {
                        var expTime = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                        if (expTime < DateTimeOffset.UtcNow)
                        {
                            throw new Exception("Token has expired");
                        }
                    }
                }

                foreach (var kvp in keyValuePairs)
                {
                    var valueStr = kvp.Value.ToString();
                    if (valueStr == null) continue;

                    if (kvp.Key == ClaimTypes.Role || kvp.Key == "role")
                    {
                        if (valueStr.StartsWith("["))
                        {
                            var parsedRoles = JsonSerializer.Deserialize<string[]>(valueStr);
                            if (parsedRoles != null)
                            {
                                claims.AddRange(parsedRoles.Select(role => new Claim(ClaimTypes.Role, role)));
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, valueStr));
                        }
                    }
                    else if (kvp.Key == "nameid" || kvp.Key == ClaimTypes.NameIdentifier)
                    {
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, valueStr));
                    }
                    else if (kvp.Key == "unique_name" || kvp.Key == ClaimTypes.Name)
                    {
                        claims.Add(new Claim(ClaimTypes.Name, valueStr));
                    }
                    else if (kvp.Key == "email")
                    {
                        claims.Add(new Claim(ClaimTypes.Email, valueStr));
                    }
                    else
                    {
                        claims.Add(new Claim(kvp.Key, valueStr));
                    }
                }
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
