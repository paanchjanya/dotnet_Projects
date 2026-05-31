using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class BookCreateUpdate
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Author { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 100000)]
        public int StockQuantity { get; set; }
        
        public string CoverImageUrl { get; set; } = string.Empty;
    }
}
