using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using EmployeeLeaveClient.Models;

namespace EmployeeLeaveClient.Services
{
    public class LeaveService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public LeaveService(HttpClient httpClient, IJSRuntime jsRuntime)
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

        public async Task<List<LeaveTypeModel>> GetLeaveTypesAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<LeaveTypeModel>>("api/leavetypes") ?? new List<LeaveTypeModel>();
            }
            catch
            {
                return new List<LeaveTypeModel>();
            }
        }

        public async Task<List<LeaveBalanceModel>> GetMyBalancesAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<LeaveBalanceModel>>("api/leavebalances/my-balances") ?? new List<LeaveBalanceModel>();
            }
            catch
            {
                return new List<LeaveBalanceModel>();
            }
        }

        public async Task<List<LeaveBalanceModel>> GetEmployeeBalancesAsync(int employeeId)
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<LeaveBalanceModel>>($"api/leavebalances/employee/{employeeId}") ?? new List<LeaveBalanceModel>();
            }
            catch
            {
                return new List<LeaveBalanceModel>();
            }
        }

        public async Task<bool> SubmitLeaveRequestAsync(MultipartFormDataContent content)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsync("api/leaverequests/apply", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<LeaveRequestModel>> GetMyRequestsAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<LeaveRequestModel>>("api/leaverequests/my-requests") ?? new List<LeaveRequestModel>();
            }
            catch
            {
                return new List<LeaveRequestModel>();
            }
        }

        public async Task<List<LeaveRequestModel>> GetPendingApprovalsAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<LeaveRequestModel>>("api/leaverequests/pending-approvals") ?? new List<LeaveRequestModel>();
            }
            catch
            {
                return new List<LeaveRequestModel>();
            }
        }

        public async Task<List<LeaveRequestModel>> GetAllRequestsAsync()
        {
            await EnsureAuthHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<List<LeaveRequestModel>>("api/leaverequests/all-requests") ?? new List<LeaveRequestModel>();
            }
            catch
            {
                return new List<LeaveRequestModel>();
            }
        }

        public async Task<bool> ActionRequestAsync(int requestId, string status, string? remarks)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/leaverequests/action/{requestId}", new
            {
                status,
                remarks
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CancelRequestAsync(int requestId)
        {
            await EnsureAuthHeaderAsync();
            var response = await _httpClient.PostAsync($"api/leaverequests/cancel/{requestId}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
