using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Helpers;
using System;

namespace Backend.Data
{
    public class BookstoreDbContext : DbContext
    {
        public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Relationships
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Book)
                .WithMany()
                .HasForeignKey(oi => oi.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@bookstore.com",
                PasswordHash = SecurityHelper.HashPassword("admin123"),
                Role = "Admin",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var customerUser = new User
            {
                Id = 2,
                Username = "customer",
                Email = "customer@bookstore.com",
                PasswordHash = SecurityHelper.HashPassword("customer123"),
                Role = "Customer",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            modelBuilder.Entity<User>().HasData(adminUser, customerUser);

            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "The Great Gatsby",
                    Author = "F. Scott Fitzgerald",
                    Description = "A novel that depicts the colorful, extravagant lives of rich people during the Roaring Twenties in America.",
                    Price = 10.99m,
                    Category = "Fiction",
                    StockQuantity = 50,
                    CoverImageUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?auto=format&fit=crop&q=80&w=400",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Book
                {
                    Id = 2,
                    Title = "To Kill a Mockingbird",
                    Author = "Harper Lee",
                    Description = "A story about racism and injustice in a small Alabama town, through the eyes of a young girl named Scout.",
                    Price = 12.99m,
                    Category = "Fiction",
                    StockQuantity = 30,
                    CoverImageUrl = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&q=80&w=400",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Book
                {
                    Id = 3,
                    Title = "1984",
                    Author = "George Orwell",
                    Description = "A classic dystopian novel about Big Brother, government surveillance, and the destruction of individuality.",
                    Price = 9.99m,
                    Category = "Dystopian",
                    StockQuantity = 40,
                    CoverImageUrl = "https://images.unsplash.com/photo-1495640388908-05fa85288e61?auto=format&fit=crop&q=80&w=400",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Book
                {
                    Id = 4,
                    Title = "A Brief History of Time",
                    Author = "Stephen Hawking",
                    Description = "An landmark volume in science writing by one of the great minds of our time, exploring the cosmos and origin of the universe.",
                    Price = 15.99m,
                    Category = "Science",
                    StockQuantity = 15,
                    CoverImageUrl = "https://images.unsplash.com/photo-1532012197267-da84d127e765?auto=format&fit=crop&q=80&w=400",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Book
                {
                    Id = 5,
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Description = "A handbook of agile software craftsmanship, filled with best practices and examples on writing clean, maintainable code.",
                    Price = 35.99m,
                    Category = "Technology",
                    StockQuantity = 25,
                    CoverImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?auto=format&fit=crop&q=80&w=400",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Book
                {
                    Id = 6,
                    Title = "The Hobbit",
                    Author = "J.R.R. Tolkien",
                    Description = "A fantasy novel about Bilbo Baggins and his epic quest to reclaim the lonely mountain and its treasure from Smaug.",
                    Price = 14.99m,
                    Category = "Fantasy",
                    StockQuantity = 20,
                    CoverImageUrl = "https://images.unsplash.com/photo-1618666012174-83b441c0bc76?auto=format&fit=crop&q=80&w=400",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
