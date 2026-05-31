using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using EmployeeLeaveClient;
using EmployeeLeaveClient.Services;
using EmployeeLeaveClient.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Base URL of the backend API
var backendUrl = builder.Configuration["BackendUrl"] ?? "http://localhost:5220/";

// Register UnauthorizedHandler
builder.Services.AddTransient<UnauthorizedHandler>();

builder.Services.AddScoped(sp => 
{
    var handler = sp.GetRequiredService<UnauthorizedHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(backendUrl) };
});

// Add custom services
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<LeaveService>();
builder.Services.AddScoped<NotificationService>();

// Add core authorization services
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
