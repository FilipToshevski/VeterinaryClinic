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
        [Display(Name = "Animal Type")]
        public string AnimalType { get; set; }

        [ValidateNever]
        public SelectList AnimalTypeOptions { get; set; }

        // Add these to properly handle vaccine forms
        public int? VaccineToAdd { get; set; }
        public int? VaccineToRemove { get; set; }
    }
}
