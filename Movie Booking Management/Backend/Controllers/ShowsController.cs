using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CineBooking.Api.Data;
using CineBooking.Api.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CineBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShowsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetShows()
        {
            var shows = await _context.Showtimes
                .Include(s => s.Movie)
                .ToListAsync();
            return Ok(shows);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddShowtime([FromBody] Showtime showtime)
        {
            if (showtime == null) return BadRequest();

            _context.Showtimes.Add(showtime);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShows), new { id = showtime.Id }, showtime);
        }

        // POST /api/shows/movie — Create a Movie together with its Showtimes
        [HttpPost("movie")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMovie([FromBody] AddMovieRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { error = "Title is required" });

            var movie = new Movie
            {
                Title = request.Title,
                Description = request.Description ?? "",
                DurationMinutes = request.DurationMinutes > 0 ? request.DurationMinutes : 120,
                PosterUrl = request.PosterUrl
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync(); // Generates movie.Id

            if (request.ShowtimeHours != null)
            {
                var today = DateTime.UtcNow.Date;
                foreach (var hour in request.ShowtimeHours)
                {
                    _context.Showtimes.Add(new Showtime
                    {
                        MovieId = movie.Id,
                        StartTime = today.AddHours(hour),
                        TicketPrice = request.TicketPrice > 0 ? request.TicketPrice : 12.0m
                    });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { movieId = movie.Id, message = "Movie added successfully" });
        }

        // DELETE /api/shows/movie/{id} — Remove a Movie and its Showtimes
        [HttpDelete("movie/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Showtimes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound(new { error = "Movie not found" });

            // Remove all associated showtimes first
            _context.Showtimes.RemoveRange(movie.Showtimes);
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Movie deleted successfully" });
        }

        [HttpGet("{movieId}/analytics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMovieAnalytics(int movieId)
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "GetMovieAnalytics";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@MovieId", movieId));

                if (command.Connection.State == ConnectionState.Closed)
                    await command.Connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var occupancy = reader["OccupancyPercentage"];
                        var revenue = reader["TotalRevenue"];

                        return Ok(new
                        {
                            OccupancyPercentage = occupancy,
                            TotalRevenue = revenue
                        });
                    }
                }
            }
            return NotFound("Analytics not found for the given movie.");
        }
    }

    public class AddMovieRequest
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int DurationMinutes { get; set; }
        public decimal TicketPrice { get; set; }
        public double[]? ShowtimeHours { get; set; } // e.g. [10, 14, 20] for 10AM, 2PM, 8PM
        public string? PosterUrl { get; set; } // Stored only in frontend for now
    }
}
