namespace BookAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookAPI.Validators;

public class Book
{
     public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MinLength(2, ErrorMessage = "Title must be at least 2 characters.")]
    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required.")]
    [MinLength(2, ErrorMessage = "Author name must be at least 2 characters.")]
    [MaxLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "Genre is required.")]
    [MaxLength(50, ErrorMessage = "Genre cannot exceed 50 characters.")]
    public string Genre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.00, 9999.99, ErrorMessage = "Price must be between $0.00 and $9999.99.")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Published date is required.")]
    [PastDate]
    public DateTime PublishedDate { get; set; }
}