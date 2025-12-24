using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VeterinaryClinic.Models;
using VeterinaryClinic.Data;
using VeterinaryClinic.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using VeterinaryClinic.Controllers;
using static VeterinaryClinic.ViewModels.ProfileViewModel;
using System.Linq;
using YourProjectNamespace.Extensions;

namespace VeterinaryClinic.Controllers
{
    [Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly VeterinaryClinicDb _context;
        private readonly UserManager<Owner> _userManager;

        private static readonly List<string> _animalTypes = new()
            {
            "Dog", "Cat", "Bird", "Rabbit",
            "Hamster", "Fish", "Reptile", "Other"
        };

        public AdminController(VeterinaryClinicDb context, UserManager<Owner> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ManagePets(string searchTerm)
        {
            var query = _userManager.Users
                .Include(u => u.Pets)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .AsQueryable();

            // Apply search filter if term exists
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(searchTerm) ||
                    u.Pets.Any(p =>
                        p.Name.ToLower().Contains(searchTerm) ||
                        p.AnimalType.ToLower().Contains(searchTerm)
                ));
            }

            var owners = await query.ToListAsync();

            var viewModel = new AdminPetsViewModel
            {
                OwnersWithPets = owners.Select(o => new OwnerWithPets
                {
                    OwnerId = o.Id,
                    OwnerName = $"{o.LastName}, {o.FirstName}",
                    OwnerEmail = o.Email,
                    Pets = o.Pets.Select(p => new PetDetails
                    {
                        PetId = p.Id,
                        Name = p.Name,
                        Age = p.Age,
                        AnimalType = p.AnimalType
                    }).ToList()
                }).ToList(),
                SearchTerm = searchTerm
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditPet(int id)
        {
            var pet = await _context.Pets
                .Include(p => p.PetVaccines)
                    .ThenInclude(pv => pv.Vaccine)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null) return NotFound();

            // Get all vaccines
            var allVaccines = await _context.Vaccines.ToListAsync();
            var petVaccineIds = pet.PetVaccines.Select(pv => pv.VaccineId).ToList();
            var availableVaccines = allVaccines
                .Where(v => !petVaccineIds.Contains(v.Id))
                .ToList();

            var viewModel = new PetEditViewModel
            {
                Id = pet.Id,
                Name = pet.Name,
                Age = pet.Age,
                AnimalType = pet.AnimalType,
                AnimalTypeOptions = new SelectList(_animalTypes)
            };

            ViewBag.CurrentVaccines = pet.PetVaccines.ToList();
            ViewBag.AvailableVaccines = availableVaccines;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPet(PetEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var pet = await _context.Pets.FindAsync(model.Id);
                    if (pet == null)
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = "Pet not found" });
                        }
                        TempData["ErrorMessage"] = "Pet not found!";
                        return RedirectToAction("ManagePets");
                    }

                    // Update pet info
                    pet.Name = model.Name;
                    pet.Age = model.Age;
                    pet.AnimalType = model.AnimalType;

                    _context.Pets.Update(pet);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, redirectUrl = Url.Action("ManagePets") });
                    }

                    TempData["SuccessMessage"] = $"Pet '{pet.Name}' updated successfully!";
                    return RedirectToAction("ManagePets");
                }
                catch (Exception ex)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Error updating pet",
                            errors = ModelState.ToDictionary(
                                k => k.Key,
                                v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                        });
                    }

                    ModelState.AddModelError("", $"Error updating pet: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while updating the pet.";
                }
            }

            // If we got here, something went wrong
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState.ToDictionary(
                        k => k.Key,
                        v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }

            // Repopulate dropdowns for non-AJAX requests
            model.AnimalTypeOptions = new SelectList(_animalTypes, model.AnimalType);
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePet(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
            {
                return NotFound();
            }

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Pet deleted successfully!";
            return RedirectToAction("ManagePets");
        }


        [HttpGet]
        public async Task<IActionResult> CreatePet()
        {
            var viewModel = new PetCreateViewModel
            {
                AnimalTypeOptions = new SelectList(_animalTypes)
            };

            await RepopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePet(PetCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var pet = new Pets
                    {
                        OwnerId = model.OwnerId,
                        Name = model.Name,
                        Age = model.Age,
                        AnimalType = model.AnimalType
                    };

                    _context.Pets.Add(pet);
                    await _context.SaveChangesAsync();

                    // Add selected vaccines
                    if (model.SelectedVaccineIds != null && model.SelectedVaccineIds.Any())
                    {
                        foreach (var vaccineId in model.SelectedVaccineIds)
                        {
                            _context.PetVaccines.Add(new PetVaccine
                            {
                                PetId = pet.Id,
                                VaccineId = vaccineId,
                                DateAdministered = DateTime.UtcNow
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Pet created successfully!";
                    return RedirectToAction("ManagePets");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating pet: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while creating the pet.";
                }
            }

            // If we got here, something went wrong - repopulate dropdowns
            await RepopulateDropdowns(model);
            return View(model);
        }

        private async Task RepopulateDropdowns(PetCreateViewModel model)
        {
            var owners = await _userManager.Users
                .OrderBy(o => o.LastName)
                .ThenBy(o => o.FirstName)
                .Select(o => new {
                    o.Id,
                    DisplayName = $"{o.LastName}, {o.FirstName} ({o.Email})"
                })
                .ToListAsync();

            var vaccines = await _context.Vaccines
                .OrderBy(v => v.Name)
                .ToListAsync();

            model.OwnerOptions = new SelectList(owners, "Id", "DisplayName", model.OwnerId);
            model.AnimalTypeOptions = new SelectList(_animalTypes, model.AnimalType);
            model.VaccineOptions = new MultiSelectList(vaccines, "Id", "Name", model.SelectedVaccineIds);
        }

        [HttpGet]
        public async Task<IActionResult> ManageVaccines()
        {
            var vaccines = await _context.Vaccines.OrderBy(v => v.Name).ToListAsync();
            return View(vaccines);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVaccine(string vaccineName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vaccineName) || vaccineName.Length < 2 || vaccineName.Length > 50)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vaccine name must be between 2-50 characters"
                    });
                }

                // Check if vaccine already exists
                var existingVaccine = await _context.Vaccines
                    .FirstOrDefaultAsync(v => v.Name.ToLower() == vaccineName.Trim().ToLower());

                if (existingVaccine != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "A vaccine with this name already exists"
                    });
                }

                var vaccine = new Vaccine { Name = vaccineName.Trim() };
                _context.Vaccines.Add(vaccine);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Vaccine '{vaccine.Name}' added successfully!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error adding vaccine to database",
                    error = ex.Message
                });
            }
        }

        // This method is for the quick add from EditPet page (existing functionality)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVaccineToPet(int petId, int vaccineId)
        {
            try
            {
                // Check if pet exists
                var pet = await _context.Pets.FindAsync(petId);
                if (pet == null)
                {
                    return Json(new { success = false, message = "Pet not found" });
                }

                // Check if vaccine exists
                var vaccine = await _context.Vaccines.FindAsync(vaccineId);
                if (vaccine == null)
                {
                    return Json(new { success = false, message = "Vaccine not found" });
                }

                // Check if vaccine is already assigned to this pet
                var existing = await _context.PetVaccines
                    .AnyAsync(pv => pv.PetId == petId && pv.VaccineId == vaccineId);

                if (existing)
                {
                    return Json(new { success = false, message = "Vaccine already assigned to this pet" });
                }

                // Add the vaccine to the pet
                var petVaccine = new PetVaccine
                {
                    PetId = petId,
                    VaccineId = vaccineId,
                    DateAdministered = DateTime.UtcNow
                };

                _context.PetVaccines.Add(petVaccine);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Vaccine added successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding vaccine", error = ex.Message });
            }
        }

        // This method is for the detailed form (with date selection)
        [HttpGet]
        public async Task<IActionResult> AddVaccineForm(int petId)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null) return NotFound();

            var availableVaccines = await _context.Vaccines
                .Where(v => !_context.PetVaccines
                    .Any(pv => pv.PetId == petId && pv.VaccineId == v.Id))
                .ToListAsync();

            var model = new AddVaccineViewModel
            {
                PetId = pet.Id,
                PetName = pet.Name,
                AvailableVaccines = new SelectList(availableVaccines, "Id", "Name"),
                DateAdministered = DateTime.UtcNow
            };

            return View("AddVaccine", model); // Specify the view name explicitly
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVaccineForm(AddVaccineViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate vaccines dropdown if returning to view
                var availableVaccines = await _context.Vaccines
                    .Where(v => !_context.PetVaccines
                        .Any(pv => pv.PetId == model.PetId && pv.VaccineId == v.Id))
                    .ToListAsync();

                model.AvailableVaccines = new SelectList(availableVaccines, "Id", "Name");
                return View("AddVaccine", model);
            }

            // Check if SelectedVaccineId has a value
            if (!model.SelectedVaccineId.HasValue || model.SelectedVaccineId.Value == 0)
            {
                ModelState.AddModelError("SelectedVaccineId", "Please select a vaccine");

                var availableVaccines = await _context.Vaccines
                    .Where(v => !_context.PetVaccines
                        .Any(pv => pv.PetId == model.PetId && pv.VaccineId == v.Id))
                    .ToListAsync();

                model.AvailableVaccines = new SelectList(availableVaccines, "Id", "Name");
                return View("AddVaccine", model);
            }

            // Check if this vaccine is already assigned to this pet
            var existingPetVaccine = await _context.PetVaccines
                .AnyAsync(pv => pv.PetId == model.PetId && pv.VaccineId == model.SelectedVaccineId.Value);

            if (existingPetVaccine)
            {
                TempData["ErrorMessage"] = "This vaccine has already been administered to this pet.";
                return RedirectToAction("EditPet", new { id = model.PetId });
            }

            var petVaccine = new PetVaccine
            {
                PetId = model.PetId,
                VaccineId = model.SelectedVaccineId.Value,
                DateAdministered = model.DateAdministered
            };

            _context.PetVaccines.Add(petVaccine);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Vaccine added successfully!";
            return RedirectToAction("EditPet", new { id = model.PetId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveVaccineFromPet(int petId, int vaccineId)
        {
            try
            {
                var petVaccine = await _context.PetVaccines
                    .FirstOrDefaultAsync(pv => pv.PetId == petId && pv.VaccineId == vaccineId);

                if (petVaccine == null)
                {
                    return Json(new { success = false, message = "Vaccine assignment not found" });
                }

                _context.PetVaccines.Remove(petVaccine);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Vaccine removed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error removing vaccine", error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVaccine(int id)
        {
            try
            {
                var vaccine = await _context.Vaccines.FindAsync(id);
                if (vaccine == null)
                {
                    TempData["ErrorMessage"] = "Vaccine not found!";
                    return RedirectToAction(nameof(ManageVaccines));
                }

                // Check if vaccine is being used by any pets
                var isInUse = await _context.PetVaccines.AnyAsync(pv => pv.VaccineId == id);
                if (isInUse)
                {
                    TempData["ErrorMessage"] = "Cannot delete vaccine - it is assigned to one or more pets!";
                    return RedirectToAction(nameof(ManageVaccines));
                }

                _context.Vaccines.Remove(vaccine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Vaccine '{vaccine.Name}' deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting vaccine: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageVaccines));
        }

        // Remove the duplicate RemoveVaccine method - use RemoveVaccineFromPet instead

        [HttpGet]
        public async Task<IActionResult> GetVaccinesTable()
        {
            var vaccines = await _context.Vaccines.OrderBy(v => v.Name).ToListAsync();
            return PartialView("_VaccinesTablePartial", vaccines);
        }

        [HttpGet]
        public async Task<IActionResult> RefreshVaccineSections(int id)
        {
            try
            {
                // Get the pet with its vaccines
                var pet = await _context.Pets
                    .Include(p => p.PetVaccines)
                        .ThenInclude(pv => pv.Vaccine)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (pet == null)
                {
                    return Json(new { success = false, message = "Pet not found" });
                }

                // Get available vaccines (ones not assigned to this pet)
                var availableVaccines = await _context.Vaccines
                    .Where(v => !pet.PetVaccines.Any(pv => pv.VaccineId == v.Id))
                    .ToListAsync();

                // Set ViewBag for the partials
                ViewBag.PetId = id;

                // Return the data to be used by JavaScript to rebuild the sections
                return Json(new
                {
                    success = true,
                    petId = id,
                    currentVaccines = pet.PetVaccines.Select(pv => new {
                        vaccineId = pv.VaccineId,
                        vaccineName = pv.Vaccine?.Name,
                        dateAdministered = pv.DateAdministered.ToShortDateString()
                    }).ToList(),
                    availableVaccines = availableVaccines.Select(v => new {
                        id = v.Id,
                        name = v.Name
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error refreshing sections", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageOwners()
        {
            var owners = await _userManager.Users
                .OrderBy(o => o.LastName)
                .ThenBy(o => o.FirstName)
                .ToListAsync();

            var viewModel = new ManageOwnersViewModel
            {
                Owners = owners.Select(o => new OwnerDetails
                {
                    Id = o.Id,
                    Email = o.Email,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    DateOfBirth = o.DateOfBirth
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOwner(ManageOwnersViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Owner
                {
                    UserName = model.Input.Email,
                    Email = model.Input.Email,
                    FirstName = model.Input.FirstName,
                    LastName = model.Input.LastName,
                    DateOfBirth = model.Input.DateOfBirth
                };

                var result = await _userManager.CreateAsync(user, model.Input.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    TempData["SuccessMessage"] = "Owner created successfully!";
                    return RedirectToAction("ManageOwners");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            var owners = await _userManager.Users.ToListAsync();
            model.Owners = owners.Select(o => new OwnerDetails
            {
                Id = o.Id,
                Email = o.Email,
                FirstName = o.FirstName,
                LastName = o.LastName,
                DateOfBirth = o.DateOfBirth
            }).ToList();

            return View("ManageOwners", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOwner(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Owner not found!";
                return RedirectToAction("ManageOwners");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Owner deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting owner!";
            }

            return RedirectToAction("ManageOwners");
        }
        [HttpGet]
        public async Task<IActionResult> EditOwner(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditOwnerViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOwner(EditOwnerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email; // Important for Identity
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.DateOfBirth = model.DateOfBirth;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Owner updated successfully!";
                    return RedirectToAction("ManageOwners");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult TestRoute()
        {
            return Json(new { message = "AdminController is working!", timestamp = DateTime.Now });
        }

        [HttpPost]
        public IActionResult TestPost()
        {
            return Json(new { message = "POST to AdminController is working!", timestamp = DateTime.Now });
        }
    }
}
