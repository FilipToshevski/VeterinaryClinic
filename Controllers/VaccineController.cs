using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VeterinaryClinic.Models;
using VeterinaryClinic.Data;
using VeterinaryClinic.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VeterinaryClinic.Controllers
{
    
    [Authorize(Roles = "Admin")]
    public class VaccineController : Controller
    {
        private readonly VeterinaryClinicDb _context;

        public VaccineController(VeterinaryClinicDb context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVaccineToPet(VaccineSelectViewModel model)
        {
            // Remove PetName from validation
            ModelState.Remove("PetName");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide valid vaccine information";
                return RedirectToAction("EditPet", "Admin", new { id = model.PetId });
            }

            
        

            try
            {
                // Check if we're adding an existing vaccine
                if (model.SelectedVaccineId.HasValue && model.SelectedVaccineId > 0)
                {
                    var existingAssociation = await _context.PetVaccines
                        .AnyAsync(pv => pv.PetId == model.PetId && pv.VaccineId == model.SelectedVaccineId);

                    if (!existingAssociation)
                    {
                        var petVaccine = new PetVaccine
                        {
                            PetId = model.PetId,
                            VaccineId = model.SelectedVaccineId.Value,
                            DateAdministered = DateTime.UtcNow
                        };
                        _context.PetVaccines.Add(petVaccine);
                        await _context.SaveChangesAsync();
                    }
                }
                // Or creating a new vaccine
                else if (!string.IsNullOrWhiteSpace(model.NewVaccineName))
                {
                    // Check if vaccine already exists
                    var existingVaccine = await _context.Vaccines
                        .FirstOrDefaultAsync(v => v.Name.ToLower() == model.NewVaccineName.Trim().ToLower());

                    if (existingVaccine == null)
                    {
                        existingVaccine = new Vaccine { Name = model.NewVaccineName.Trim() };
                        _context.Vaccines.Add(existingVaccine);
                        await _context.SaveChangesAsync(); // Save to get ID
                    }

                    // Check if pet already has this vaccine
                    var existingAssociation = await _context.PetVaccines
                        .AnyAsync(pv => pv.PetId == model.PetId && pv.VaccineId == existingVaccine.Id);

                    if (!existingAssociation)
                    {
                        var petVaccine = new PetVaccine
                        {
                            PetId = model.PetId,
                            VaccineId = existingVaccine.Id,
                            DateAdministered = DateTime.UtcNow
                        };
                        _context.PetVaccines.Add(petVaccine);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Vaccine added successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding vaccine: {ex.Message}";
            }

            return RedirectToAction("EditPet", "Admin", new { id = model.PetId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveVaccine(int petId, int vaccineId)
        {
            var petVaccine = await _context.PetVaccines
                .FirstOrDefaultAsync(pv => pv.PetId == petId && pv.VaccineId == vaccineId);

            if (petVaccine != null)
            {
                _context.PetVaccines.Remove(petVaccine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vaccine removed successfully!";
            }

            return RedirectToAction("EditPet", "Admin", new { id = petId });
        }
    }
}
