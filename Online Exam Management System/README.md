# Online Exam Management System

Full-stack online exam application with an ASP.NET Core API, Blazor WebAssembly client, shared DTOs, EF Core data layer, Redis-ready distributed caching, JWT authentication, MudBlazor UI, Chart.js interop, and SignalR leaderboard updates.

## Projects

- `OnlineExam.API` - Web API controllers, JWT auth, SignalR hub, Redis/distributed cache wiring, scoring logic.
- `OnlineExam.BlazorUI` - Blazor WebAssembly client with MudBlazor screens, timer, keyboard shortcuts, charts, confetti, and admin question creation.
- `OnlineExam.Shared` - DTOs and enums shared by API and UI.
- `OnlineExam.Data` - EF Core entities, `ApplicationDbContext`, password hashing, and seed data.
- `OnlineExam.Tests` - xUnit test project scaffold.

## Notes

This machine does not have .NET 8 templates installed, so the solution is pinned to the installed .NET 9 SDK in `global.json`. The architecture, package choices, and code structure follow the requested .NET 8 plan and can be retargeted to `net8.0` on a machine with the .NET 8 SDK/targeting packs.

## Seeded Logins

- Admin: `admin@exam.com` / `Admin123!`
- Student: `student@exam.com` / `Student123!`

## Run

```powershell
cd "C:\Users\desai\Desktop\Sources\Transformers\Jazz\Online Exam Management System"
dotnet restore
dotnet build OnlineExamSolution.slnx -m:1
dotnet run --project OnlineExam.API --launch-profile https
dotnet run --project OnlineExam.BlazorUI --launch-profile https
```

Open the Blazor client at `https://localhost:5002`.

The API uses SQL Server LocalDB by default:

```json
"Default": "Server=(localdb)\\mssqllocaldb;Database=OnlineExamDb;Trusted_Connection=true;MultipleActiveResultSets=true"
```

Redis is optional. Set `ConnectionStrings:Redis` in `OnlineExam.API/appsettings.json` to enable StackExchangeRedis distributed caching; otherwise the API uses in-memory distributed cache for local development.

## Implemented Features

- JWT login/register with role claims.
- Active exam listing.
- Cached question retrieval with admin invalidation.
- Attempt start, answer save, auto correctness calculation, final submit, timeout status.
- Result summary with per-question review, topic charts, share-to-clipboard, and pass confetti.
- MudBlazor bright/light theme and dark-mode toggle saved in localStorage.
- Exam timer, progress indicator, animated answer options, keyboard shortcuts `1`-`4` and `Enter`.
- SignalR leaderboard hub and client subscription.
- Admin-protected question creation with dynamic options and one-correct-answer validation.
