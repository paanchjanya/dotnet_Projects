using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Book
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Book Title is mandatory.")]
    [StringLength(200, ErrorMessage = "Book Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author Name is mandatory.")]
    [StringLength(100, ErrorMessage = "Author Name cannot exceed 100 characters.")]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is mandatory.")]
    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
    public string Category { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Published Date is mandatory.")]
    [PastOrPresentDate(ErrorMessage = "Published date cannot be a future date.")]
    public DateOnly PublishedDate { get; set; }


    public bool IsAvailable { get; set; }
}

public class PastOrPresentDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateOnly dateValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (dateValue > today)
            {
                return new ValidationResult(ErrorMessage ?? "Published date cannot be in the future.");
            }
        }
        else if (value is DateTime dateTimeValue)
        {
            if (dateTimeValue.Date > DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "Published date cannot be in the future.");
            }
        }
        return ValidationResult.Success;
    }
}
