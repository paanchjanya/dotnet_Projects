using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EmployeeLeaveClient.Shared
{
    public class UnauthorizedHandler : DelegatingHandler
    {
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public UnauthorizedHandler(NavigationManager navigationManager, IJSRuntime jsRuntime)
        {
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            // If the backend returns 401 Unauthorized, and it's not a login attempt
            if (response.StatusCode == HttpStatusCode.Unauthorized && 
                request.RequestUri != null && 
                !request.RequestUri.AbsolutePath.Contains("/auth/login", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                }
                catch { }
                
                _navigationManager.NavigateTo("login");
            }

            return response;
        }
    }
}
