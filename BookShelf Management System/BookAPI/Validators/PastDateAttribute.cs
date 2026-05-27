using System.ComponentModel.DataAnnotations;

namespace BookAPI.Validators;

public class PastDateAttribute : ValidationAttribute
{
    public PastDateAttribute()
    {
        ErrorMessage = "Published date cannot be a future date.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is DateTime date)
        {
            if (date.Date > DateTime.Today)
                return new ValidationResult(ErrorMessage);
        }
        return ValidationResult.Success;
    }
}