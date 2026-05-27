using BookAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            // Title
            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(150);

            // Author
            entity.Property(b => b.Author)
                .IsRequired()
                .HasMaxLength(100);

            // Genre
            entity.Property(b => b.Genre)
                .IsRequired()
                .HasMaxLength(50);

            // Price — DB level: price >= 0
            entity.Property(b => b.Price)
                .HasColumnType("decimal(10,2)");

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Books_Price", "[Price] >= 0"
            ));

            // PublishedDate — DB level: cannot be future date
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Books_PublishedDate", "[PublishedDate] <= GETDATE()"
            ));
        });
    }
}