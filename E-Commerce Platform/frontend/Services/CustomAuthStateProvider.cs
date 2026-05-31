using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace frontend.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly AuthenticationState _anonymous;

        public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return _anonymous;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            try
            {
                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch (Exception)
            {
                // Token might be corrupted
                await MarkUserAsLoggedOut();
                return _anonymous;
            }
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var authenticatedUser = new ClaimsPrincipal(identity);
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            
            NotifyAuthenticationStateChanged(authState);
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            var authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];

            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                // Look for role claim keys
                keyValuePairs.TryGetValue(ClaimTypes.Role, out object? roles);
                if (roles == null)
                {
                    keyValuePairs.TryGetValue("role", out roles);
                }

                if (roles != null)
                {
                    var rolesStr = roles.ToString()!.Trim();
                    if (rolesStr.StartsWith("["))
                    {
                        var parsedRoles = JsonSerializer.Deserialize<string[]>(rolesStr);
                        foreach (var parsedRole in parsedRoles!)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rolesStr));
                    }
                }

                // Look for name claim keys
                keyValuePairs.TryGetValue(ClaimTypes.Name, out object? username);
                if (username == null)
                {
                    keyValuePairs.TryGetValue("unique_name", out username);
                }
                if (username != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, username.ToString()!));
                }

                // Look for NameIdentifier key
                keyValuePairs.TryGetValue(ClaimTypes.NameIdentifier, out object? nameId);
                if (nameId == null)
                {
                    keyValuePairs.TryGetValue("sub", out nameId);
                }
                if (nameId != null)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId.ToString()!));
                }

                // Add other claims
                foreach (var kvp in keyValuePairs)
                {
                    var key = kvp.Key;
                    // Skip keys we already handled in claims
                    if (key == ClaimTypes.Role || key == "role" || 
                        key == ClaimTypes.Name || key == "unique_name" || 
                        key == ClaimTypes.NameIdentifier || key == "sub")
                    {
                        continue;
                    }
                    claims.Add(new Claim(key, kvp.Value.ToString() ?? ""));
                }
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
}
