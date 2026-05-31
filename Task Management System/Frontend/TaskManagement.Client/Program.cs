using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskManagement.Client;
using TaskManagement.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient pointing to the backend Web API (port 5204)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5204/") });

// Register custom services
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ThemeManager>();
builder.Services.AddScoped<BoardService>();

// Register authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateProvider>());

await builder.Build().RunAsync();
