using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VeterinaryClinic.ViewModels
{
    
    public class VaccineSelectViewModel
    {
        public int PetId { get; set; }

        public required string PetName { get; set; }

        [Display(Name = "Existing Vaccines")]
        public List<SelectListItem> ExistingVaccines { get; set; } = new List<SelectListItem>();

        [Display(Name = "New Vaccine")]
        [StringLength(100)]
        public string? NewVaccineName { get; set; }
        public int? SelectedVaccineId { get; set; }
    }
}
