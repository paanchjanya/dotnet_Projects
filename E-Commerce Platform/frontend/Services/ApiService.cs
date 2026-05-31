using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using frontend.Models;
using System.Net.Http.Headers;

namespace frontend.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authStateProvider;

        public string BaseAddress => _http.BaseAddress?.ToString() ?? "http://localhost:5124/";

        public ApiService(HttpClient http, CustomAuthStateProvider authStateProvider)
        {
            _http = http;
            _authStateProvider = authStateProvider;
        }

        private async Task EnsureAuthHeaderAsync()
        {
            var token = await _authStateProvider.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }
        }

        // Auth API
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", request);
                if (response.IsSuccessStatusCode)
                {
                    var authRes = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (authRes != null)
                    {
                        await _authStateProvider.MarkUserAsAuthenticated(authRes.Token);
                        return authRes;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/register", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _authStateProvider.MarkUserAsLoggedOut();
        }

        // Books API
        public async Task<List<Book>> GetBooksAsync(string? search = null, string? category = null)
        {
            try
            {
                var url = "api/books";
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={System.Net.WebUtility.UrlEncode(search)}");
                if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={System.Net.WebUtility.UrlEncode(category)}");
                
                if (queryParams.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams);
                }

                return await _http.GetFromJsonAsync<List<Book>>(url) ?? new List<Book>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching books: {ex.Message}");
                return new List<Book>();
            }
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<Book>($"api/books/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching book by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<Book?> CreateBookAsync(BookCreateUpdate book)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                var response = await _http.PostAsJsonAsync("api/books", book);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Book>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating book: {ex.Message}");
            }
            return null;
        }

        public async Task<Book?> UpdateBookAsync(int id, BookCreateUpdate book)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                var response = await _http.PutAsJsonAsync($"api/books/{id}", book);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Book>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                var response = await _http.DeleteAsync($"api/books/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting book: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> UploadCoverImageAsync(IBrowserFile file)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                using var content = new MultipartFormDataContent();
                
                // Limit file size to 5MB
                var fileStream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                
                content.Add(streamContent, "file", file.Name);

                var response = await _http.PostAsync("api/books/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                    return result?.Url;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading cover image: {ex.Message}");
            }
            return null;
        }

        private class UploadResult
        {
            public string Url { get; set; } = string.Empty;
        }

        // Orders API
        public async Task<bool> CreateOrderAsync(OrderCreate order)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                var response = await _http.PostAsJsonAsync("api/orders", order);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                return false;
            }
        }

        public async Task<List<OrderResponse>> GetAllOrdersAsync()
        {
            try
            {
                await EnsureAuthHeaderAsync();
                return await _http.GetFromJsonAsync<List<OrderResponse>>("api/orders") ?? new List<OrderResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all orders: {ex.Message}");
                return new List<OrderResponse>();
            }
        }

        public async Task<List<OrderResponse>> GetMyOrdersAsync()
        {
            try
            {
                await EnsureAuthHeaderAsync();
                return await _http.GetFromJsonAsync<List<OrderResponse>>("api/orders/my-orders") ?? new List<OrderResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user orders: {ex.Message}");
                return new List<OrderResponse>();
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                await EnsureAuthHeaderAsync();
                var response = await _http.PutAsJsonAsync($"api/orders/{orderId}/status", status);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order status: {ex.Message}");
                return false;
            }
        }

        // Dashboard API
        public async Task<DashboardStats?> GetDashboardStatsAsync()
        {
            try
            {
                await EnsureAuthHeaderAsync();
                return await _http.GetFromJsonAsync<DashboardStats>("api/dashboard/stats");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching dashboard stats: {ex.Message}");
                return null;
            }
        }
    }
}
