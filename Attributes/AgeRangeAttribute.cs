namespace VeterinaryClinic.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

public class AgeRangeAttribute : ValidationAttribute
{
    private readonly int _minAge;
    private readonly int _maxAge;

    public AgeRangeAttribute(int minAge, int maxAge)
    {
        _minAge = minAge;
        _maxAge = maxAge;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;

            // Adjust age if the birthday hasn't occurred yet this year.
            if (dob > today.AddYears(-age))
                age--;

            if (age < _minAge)
            {
                return new ValidationResult($"You must be at least {_minAge} years old.");
            }
            if (age > _maxAge)
            {
                return new ValidationResult($"You must be {_maxAge} years old or younger.");
            }

            return ValidationResult.Success;
        }

        return new ValidationResult("Invalid date format.");
    }
}
