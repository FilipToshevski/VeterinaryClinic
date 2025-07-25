using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VeterinaryClinic.ViewModels
{
    public class AddVaccineViewModel
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int? SelectedVaccineId { get; set; }
        public SelectList AvailableVaccines { get; set; }
        public DateTime DateAdministered { get; set; } = DateTime.UtcNow;
    }
}
