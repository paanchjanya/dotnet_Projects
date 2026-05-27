using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CineBooking.Api.Models
{
    public class Movie
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public int DurationMinutes { get; set; }
        
        public string? PosterUrl { get; set; }
        
        public ICollection<Showtime> Showtimes { get; set; }
    }
}
