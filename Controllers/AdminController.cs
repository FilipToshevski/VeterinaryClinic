using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VeterinaryClinic.Models;
using VeterinaryClinic.Data;
using VeterinaryClinic.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using VeterinaryClinic.Controllers;

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

            // Get ALL vaccines (not just unused ones) for the dropdown
            var allVaccines = await _context.Vaccines.ToListAsync();

            var viewModel = new PetEditViewModel
            {
                Id = pet.Id,
                Name = pet.Name,
                Age = pet.Age,
                AnimalType = pet.AnimalType,
                AnimalTypeOptions = new SelectList(new List<string>
        {
            "Dog", "Cat", "Bird", "Rabbit",
            "Hamster", "Fish", "Reptile", "Other"
        }),
                CurrentVaccines = pet.PetVaccines.Select(pv => new VaccineViewModel
                {
                    Id = pv.Vaccine.Id,
                    Name = pv.Vaccine.Name,
                    DateAdministered = pv.DateAdministered
                }).ToList(),
                VaccineSelection = new VaccineSelectViewModel
                {
                    PetId = pet.Id,
                    PetName = pet.Name,
                    ExistingVaccines = allVaccines
                        .Where(v => !pet.PetVaccines.Any(pv => pv.VaccineId == v.Id))
                        .Select(v => new SelectListItem
                        {
                            Value = v.Id.ToString(),
                            Text = v.Name
                        }).ToList()
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPet(PetEditViewModel model)
        {
            ModelState.Remove("VaccineSelection.PetName");

            if (ModelState.IsValid)
            {
                try
                {
                    var pet = await _context.Pets
                        .Include(p => p.PetVaccines)
                        .FirstOrDefaultAsync(p => p.Id == model.Id);

                    if (pet == null) return NotFound();

                    // Update pet details
                    pet.Name = model.Name;
                    pet.Age = model.Age;
                    pet.AnimalType = model.AnimalType;

                    // Handle vaccine additions from dropdown (existing vaccines only)
                    if (model.VaccineSelection.SelectedVaccineId.HasValue)
                    {
                        var vaccineId = model.VaccineSelection.SelectedVaccineId.Value;
                        var existingAssociation = await _context.PetVaccines
                            .AnyAsync(pv => pv.PetId == pet.Id && pv.VaccineId == vaccineId);

                        if (!existingAssociation)
                        {
                            _context.PetVaccines.Add(new PetVaccine
                            {
                                PetId = pet.Id,
                                VaccineId = vaccineId,
                                DateAdministered = DateTime.UtcNow
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Pet updated successfully!";
                    return RedirectToAction("ManagePets");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating pet: {ex.Message}");
                }
            }

            // Repopulate dropdowns if returning to view
            var allVaccines = await _context.Vaccines.ToListAsync();
            model.VaccineSelection.ExistingVaccines = allVaccines
                .Where(v => !model.CurrentVaccines.Any(cv => cv.Id == v.Id))
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.Name
                }).ToList();

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
            var owners = await _context.Users
                .Where(o => o.FirstName != null && o.LastName != null)
                .OrderBy(o => o.LastName)
                .ThenBy(o => o.FirstName)
                .ToListAsync();

            var vaccines = await _context.Vaccines.OrderBy(v => v.Name).ToListAsync();

            var viewModel = new PetCreateViewModel
            {
                OwnerOptions = new SelectList(
                    owners.Select(o => new {
                        o.Id,
                        DisplayName = $"{o.LastName}, {o.FirstName} ({o.Email})"
                    }),
                    "Id",
                    "DisplayName"
                ),
                AnimalTypeOptions = new SelectList(_animalTypes),
                VaccineOptions = new MultiSelectList(vaccines, "Id", "Name")
            };

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
                    // Create the pet first
                    var pet = new Pet
                    {
                        OwnerId = model.OwnerId,
                        Name = model.Name,
                        Age = model.Age,
                        AnimalType = model.AnimalType
                    };

                    _context.Pets.Add(pet);
                    await _context.SaveChangesAsync(); // Save to get the pet ID

                    // Handle vaccine associations
                    if (model.SelectedVaccineIds != null && model.SelectedVaccineIds.Any())
                    {
                        foreach (var vaccineId in model.SelectedVaccineIds)
                        {
                            // Check if vaccine exists
                            var vaccineExists = await _context.Vaccines.AnyAsync(v => v.Id == vaccineId);
                            if (vaccineExists)
                            {
                                _context.PetVaccines.Add(new PetVaccine
                                {
                                    PetId = pet.Id,
                                    VaccineId = vaccineId,
                                    DateAdministered = DateTime.UtcNow
                                });
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Pet created successfully!";
                    return RedirectToAction("Profile", "Account");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving pet: {ex.Message}");
                }
            }

            // Repopulate dropdowns if validation fails
            await RepopulateDropdowns(model);
            return View(model);
        }

        private async Task RepopulateDropdowns(PetCreateViewModel model)
        {
            var owners = await _context.Users
                .Where(o => o.FirstName != null && o.LastName != null)
                .OrderBy(o => o.LastName)
                .ThenBy(o => o.FirstName)
                .ToListAsync();

            var vaccines = await _context.Vaccines.OrderBy(v => v.Name).ToListAsync();

            model.OwnerOptions = new SelectList(
                owners.Select(o => new {
                    o.Id,
                    DisplayName = $"{o.LastName}, {o.FirstName} ({o.Email})"
                }),
                "Id",
                "DisplayName",
                model.OwnerId
            );

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
        public async Task<IActionResult> AddVaccine(string vaccineName)
        {
            if (!string.IsNullOrWhiteSpace(vaccineName))
            {
                var vaccine = new Vaccine { Name = vaccineName.Trim() };
                _context.Vaccines.Add(vaccine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vaccine added successfully!";
            }
            return RedirectToAction("ManageVaccines");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVaccine(int id)
        {
            var vaccine = await _context.Vaccines.FindAsync(id);
            if (vaccine != null)
            {
                _context.Vaccines.Remove(vaccine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vaccine deleted successfully!";
            }
            return RedirectToAction("ManageVaccines");
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

            return RedirectToAction("EditPet", new { id = petId });
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
    }
}
