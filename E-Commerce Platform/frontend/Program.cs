using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using frontend;
using frontend.Services;
using System;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Point HttpClient to backend Web API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5124/") });

// Register Auth Services
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// Register App Services
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
