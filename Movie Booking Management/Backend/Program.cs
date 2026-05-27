using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using CineBooking.Api.Data;
using CineBooking.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", builder =>
        builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader());
});

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "ThisIsAVerySecretKeyForJwtAuthentication123456");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Use Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated(); // Ensure DB is created
    
    // Add PosterUrl column if it doesn't exist (EnsureCreated won't add columns to existing tables)
    try
    {
        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Movies') AND name = 'PosterUrl')
            BEGIN
                ALTER TABLE Movies ADD PosterUrl NVARCHAR(MAX) NULL
            END
        ");
    }
    catch { /* Column might already exist */ }
    
    if (!context.Movies.Any())
    {
        var m1 = new CineBooking.Api.Models.Movie 
        { 
            Title = "Avengers: Quantum Paradox", 
            Description = "Sci-fi Action", 
            DurationMinutes = 150 
        };
        var m2 = new CineBooking.Api.Models.Movie 
        { 
            Title = "Interstellar Resurgence", 
            Description = "Space Odyssey", 
            DurationMinutes = 165 
        };
        var m3 = new CineBooking.Api.Models.Movie 
        { 
            Title = "Dune: The Golden Path", 
            Description = "Sci-fi Epic", 
            DurationMinutes = 180 
        };

        context.Movies.AddRange(m1, m2, m3);
        context.SaveChanges();

        var today = DateTime.UtcNow.Date.AddHours(10);
        context.Showtimes.AddRange(
            new CineBooking.Api.Models.Showtime { MovieId = m1.Id, StartTime = today, TicketPrice = 12.0m },
            new CineBooking.Api.Models.Showtime { MovieId = m1.Id, StartTime = today.AddHours(3.5), TicketPrice = 12.0m },
            new CineBooking.Api.Models.Showtime { MovieId = m2.Id, StartTime = today.AddHours(1), TicketPrice = 15.0m },
            new CineBooking.Api.Models.Showtime { MovieId = m3.Id, StartTime = today.AddHours(4), TicketPrice = 15.0m }
        );
        context.SaveChanges();
    }
    
    // Seed Admin User
    if (!context.Users.Any(u => u.Role == "Admin"))
    {
        var admin = new CineBooking.Api.Models.User
        {
            Username = "admin",
            PasswordHash = "admin123", // In a real app, hash this!
            Role = "Admin",
            CreditBalance = 0
        };
        context.Users.Add(admin);
        context.SaveChanges();
    }
}

app.Run();
