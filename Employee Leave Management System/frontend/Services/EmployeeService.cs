using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Authorization;
using EmployeeLeaveClient.Models;
using EmployeeLeaveClient.Shared;

namespace EmployeeLeaveClient.Services
{
    public class EmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthenticationStateProvider _authStateProvider;

        public EmployeeService(HttpClient httpClient, IJSRuntime jsRuntime, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _authStateProvider = authStateProvider;
        }

        private async Task EnsureAuthHeaderAsync()
        {
            if (_httpClient.DefaultRequestHeaders.Authorization == null)
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        public async Task<UserModel?> GetProfileAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<UserModel>("api/employees/profile");
            }
            catch
            {
                return null;
            }
        }

        public async Task<UserModel?> GetEmployeeByIdAsync(int id)
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<UserModel>($"api/employees/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateProfileAsync(string firstName, string lastName, string email, string department, int? managerId)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync("api/employees/profile", new
            {
                firstName,
                lastName,
                email,
                department,
                managerId
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (result.TryGetProperty("token", out var tokenProp))
                {
                    var token = tokenProp.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        ((ApiAuthenticationStateProvider)_authStateProvider).MarkUserAsAuthenticated(token);
                    }
                }
                return true;
            }
            return false;
        }

        public async Task<List<UserModel>> GetAllEmployeesAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<UserModel>>("api/employees") ?? new List<UserModel>();
            }
            catch
            {
                return new List<UserModel>();
            }
        }

        public async Task<List<object>> GetManagersAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<object>>("api/employees/managers") ?? new List<object>();
            }
            catch
            {
                return new List<object>();
            }
        }

        public async Task<string?> UploadProfilePictureAsync(MultipartFormDataContent content)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsync("api/employees/profile-picture", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                var dbPath = result.GetProperty("profilePictureUrl").GetString();
                
                if (result.TryGetProperty("token", out var tokenProp))
                {
                    var token = tokenProp.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        ((ApiAuthenticationStateProvider)_authStateProvider).MarkUserAsAuthenticated(token);
                    }
                }
                return dbPath;
            }
            return null;
        }
    }
}
