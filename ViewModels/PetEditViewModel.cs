using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VeterinaryClinic.ViewModels
{
    public class PetEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a pet name")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [Range(0, 50, ErrorMessage = "Age must be between 0 and 50")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Please specify animal type")]
        public string AnimalType { get; set; }
        
        [ValidateNever]
        public SelectList AnimalTypeOptions { get; set; }

        public List<VaccineViewModel> CurrentVaccines { get; set; } = new List<VaccineViewModel>();
        public VaccineSelectViewModel VaccineSelection { get; set; } 
    }


    public class VaccineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateAdministered { get; set; }
    }
}