using Microsoft.EntityFrameworkCore;
using CineBooking.Api.Models;

namespace CineBooking.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<TicketDetail> TicketDetails { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite unique constraint to prevent double booking physically at DB level
            modelBuilder.Entity<TicketDetail>()
                .HasIndex(t => new { t.ShowtimeId, t.RowNumber, t.SeatNumber })
                .IsUnique();

            // Efficient Indexing Strategy
            modelBuilder.Entity<Showtime>()
                .HasIndex(s => s.MovieId);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.ShowtimeId);

            modelBuilder.Entity<TicketDetail>()
                .HasIndex(t => t.ShowtimeId);

            // Prevent cascade delete issues
            modelBuilder.Entity<TicketDetail>()
                .HasOne(t => t.Showtime)
                .WithMany()
                .HasForeignKey(t => t.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Showtime)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
