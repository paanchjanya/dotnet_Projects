using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineBooking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOccupancyStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
                CREATE PROCEDURE GetMovieAnalytics
                    @MovieId INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @TotalSeats INT;
                    DECLARE @TotalShowtimes INT;
                    
                    SELECT @TotalSeats = COUNT(*) FROM Seats;
                    SELECT @TotalShowtimes = COUNT(*) FROM Showtimes WHERE MovieId = @MovieId;

                    DECLARE @TotalCapacity INT = @TotalSeats * @TotalShowtimes;
                    
                    DECLARE @BookedSeats INT;
                    SELECT @BookedSeats = COUNT(*)
                    FROM TicketDetails td
                    JOIN Showtimes s ON td.ShowtimeId = s.Id
                    WHERE s.MovieId = @MovieId;

                    DECLARE @OccupancyPercentage DECIMAL(5,2) = 0;
                    IF @TotalCapacity > 0
                    BEGIN
                        SET @OccupancyPercentage = (CAST(@BookedSeats AS DECIMAL) / @TotalCapacity) * 100.0;
                    END

                    DECLARE @TotalRevenue DECIMAL(18,2);
                    SELECT @TotalRevenue = ISNULL(SUM(b.TotalAmount), 0)
                    FROM Bookings b
                    JOIN Showtimes s ON b.ShowtimeId = s.Id
                    WHERE s.MovieId = @MovieId;

                    SELECT 
                        @OccupancyPercentage AS OccupancyPercentage,
                        @TotalRevenue AS TotalRevenue;
                END
            ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetMovieAnalytics;");
        }
    }
}
