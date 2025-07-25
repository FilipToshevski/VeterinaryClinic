using System.ComponentModel.DataAnnotations;

namespace VeterinaryClinic.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Registered Email")]
        public string Email { get; set; }
    }
}