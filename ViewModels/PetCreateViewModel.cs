using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VeterinaryClinic.ViewModels
{
    public class PetCreateViewModel
    {
        [Required(ErrorMessage = "Please select an owner")]
        [Display(Name = "Owner")]
        public string OwnerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a pet name")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, 50, ErrorMessage = "Age must be between 0 and 50")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Please specify animal type")]
        [Display(Name = "Animal Type")]
        //[StringLength(50, MinimumLength = 2, ErrorMessage = "Must be between 2-50 characters")]
        public string AnimalType { get; set; } = string.Empty;

        //For dropdown:
      
        public SelectList? OwnerOptions { get; set; }
        public SelectList? AnimalTypeOptions { get; set; }

        [Display(Name = "Vaccines")]
        public List<int> SelectedVaccineIds { get; set; } = new List<int>();
        [ValidateNever]
        public MultiSelectList VaccineOptions { get; set; }
    }
}
