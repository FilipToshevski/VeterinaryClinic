using System;
using System.ComponentModel.DataAnnotations;
using VeterinaryClinic.Attributes;

namespace VeterinaryClinic.View_Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [AgeRange(18, 100)]  // This ensures the age is between 18 and 100.
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords need to match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
