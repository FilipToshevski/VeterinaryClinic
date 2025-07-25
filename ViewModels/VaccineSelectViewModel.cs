using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VeterinaryClinic.ViewModels
{
    
    public class VaccineSelectViewModel
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public List<SelectListItem> ExistingVaccines { get; set; } = new();
        public int? SelectedVaccineId { get; set; }


        [Display(Name = "Or Create New Vaccine")]
        [StringLength(100, ErrorMessage = "Vaccine name cannot exceed 100 characters")]
        public string? NewVaccineName { get; set; }
    }
}

