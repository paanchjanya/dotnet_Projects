using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using EmployeeLeaveClient.Models;

namespace EmployeeLeaveClient.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public NotificationService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        private async Task EnsureAuthHeaderAsync()
        {
            if (_httpClient.DefaultRequestHeaders.Authorization == null)
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                }
            }
        }

        public async Task<List<NotificationModel>> GetMyNotificationsAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<NotificationModel>>("api/notifications/my-notifications") ?? new List<NotificationModel>();
            }
            catch
            {
                return new List<NotificationModel>();
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"api/notifications/mark-read/{notificationId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsync("api/notifications/mark-all-read", null);
            return response.IsSuccessStatusCode;
        }
    }
}
