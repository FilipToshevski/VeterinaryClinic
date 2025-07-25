using System.ComponentModel.DataAnnotations;
using VeterinaryClinic.Models;

namespace VeterinaryClinic.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Age")]
        public int? CalculatedAge => DateOfBirth.HasValue ?
            DateTime.Now.Year - DateOfBirth.Value.Year : null;

        public List<PetWithVaccines> Pets { get; set; } = new List<PetWithVaccines>();

        public class PetWithVaccines
        {
            public int PetId { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public string AnimalType { get; set; }
            public List<VaccineViewModel> Vaccines { get; set; } = new List<VaccineViewModel>();
        }

       
    }
}