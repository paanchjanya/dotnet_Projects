using backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();


// Register SQL Server DB Context
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the SQL Book Repository
builder.Services.AddScoped<IBookRepository, SqlBookRepository>();

// Configure CORS for Angular development server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS policy
app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();    // endpoint exution

// Database Auto-Creation and Seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    context.Database.EnsureCreated();
    
    if (!context.Books.Any())
    {
        context.Books.AddRange(
            new backend.Models.Book { Title = "Clean Code", Author = "Robert Martin", Category = "Programming", Price = 599.00M, PublishedDate = new DateOnly(2008, 8, 1), IsAvailable = true },
            new backend.Models.Book { Title = "The Pragmatic Programmer", Author = "Andy Hunt & Dave Thomas", Category = "Programming", Price = 649.00M, PublishedDate = new DateOnly(1999, 10, 30), IsAvailable = true },
            new backend.Models.Book { Title = "Introduction to Algorithms", Author = "Thomas H. Cormen", Category = "Algorithms", Price = 1200.00M, PublishedDate = new DateOnly(2009, 7, 31), IsAvailable = true },
            new backend.Models.Book { Title = "Captain America The Winter Soldier", Author = "Praveen Desai", Category = "Fiction", Price = 499.00M, PublishedDate = new DateOnly(2014, 4, 4), IsAvailable = false }
        );
        context.SaveChanges();
    }
}

app.Run();
