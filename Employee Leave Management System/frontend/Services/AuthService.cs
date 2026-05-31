using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using EmployeeLeaveClient.Models;
using EmployeeLeaveClient.Shared;

namespace EmployeeLeaveClient.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> Register(string username, string password, string email, string firstName, string lastName, string department, string role, int? managerId)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
            {
                username,
                password,
                email,
                firstName,
                lastName,
                department,
                role,
                managerId
            });

            return response.IsSuccessStatusCode;
        }

        public async Task<string?> Login(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
            {
                username,
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var token = result.GetProperty("token").GetString();

            if (!string.IsNullOrEmpty(token))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
                
                // Configure Auth header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                ((ApiAuthenticationStateProvider)_authStateProvider).MarkUserAsAuthenticated(token);
                return token;
            }

            return null;
        }

        public async Task Logout()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            ((ApiAuthenticationStateProvider)_authStateProvider).MarkUserAsLoggedOut();
        }
    }
}
