using System.ComponentModel.DataAnnotations;

namespace VeterinaryClinic.View_Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "You need an email address to login.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "You need an valid password to login.")]
        [DataType(DataType.Password)]

        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}
