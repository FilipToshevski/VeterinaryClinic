using System.ComponentModel.DataAnnotations;

namespace VeterinaryClinic.ViewModels
{
    
    public class ManageOwnersViewModel
    {
        public List<OwnerDetails> Owners { get; set; } = new List<OwnerDetails>();
        public OwnerInputModel Input { get; set; } = new OwnerInputModel();
    }

    public class OwnerDetails
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class OwnerInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }
}
