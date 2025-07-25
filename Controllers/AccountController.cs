using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryClinic.Data;
using VeterinaryClinic.Models;
using VeterinaryClinic.View_Models;
using VeterinaryClinic.ViewModels;

namespace VeterinaryClinic.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<Owner> _ownerManager;
        private readonly SignInManager<Owner> _signInManager;
        private readonly VeterinaryClinicDb _context;

        public AccountController(UserManager<Owner> ownerManager, SignInManager<Owner> signInManager, VeterinaryClinicDb context)
        {
            _ownerManager = ownerManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Owner
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    
                };

                var result = await _ownerManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                   
                    await _ownerManager.AddToRoleAsync(user, "User"); 

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Profile", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Profile", "Account");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _ownerManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Pets = await _context.Pets
                    .Where(p => p.OwnerId == user.Id)
                    .Include(p => p.PetVaccines)
                        .ThenInclude(pv => pv.Vaccine)
                    .Select(p => new ProfileViewModel.PetWithVaccines
                    {
                        PetId = p.Id,
                        Name = p.Name,
                        Age = p.Age,
                        AnimalType = p.AnimalType,
                        Vaccines = p.PetVaccines.Select(pv => new VaccineViewModel
                        {
                            VaccineId = pv.VaccineId,
                            Name = pv.Vaccine.Name,
                            DateAdministered = pv.DateAdministered
                        }).ToList()
                    }).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate pets with PetWithVaccines if validation fails
                var currentUser = await _ownerManager.GetUserAsync(User);
                model.Pets = await _context.Pets
                    .Where(p => p.OwnerId == currentUser.Id)
                    .Include(p => p.PetVaccines)
                        .ThenInclude(pv => pv.Vaccine)
                    .Select(p => new ProfileViewModel.PetWithVaccines
                    {
                        PetId = p.Id,
                        Name = p.Name,
                        Age = p.Age,
                        AnimalType = p.AnimalType,
                        Vaccines = p.PetVaccines.Select(pv => new VaccineViewModel
                        {
                            VaccineId = pv.VaccineId,
                            Name = pv.Vaccine.Name,
                            DateAdministered = pv.DateAdministered
                        }).ToList()
                    }).ToListAsync();
                return View(model);
            }

            // Rest of your code remains the same...
            var user = await _ownerManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;

            var result = await _ownerManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _ownerManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _ownerManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user doesn't exist
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var code = await _ownerManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account",
                    new { email = model.Email, code }, protocol: Request.Scheme);

                // TODO: Implement email sending service here
                // For now, we'll display the link (remove in production)
                TempData["ResetLink"] = callbackUrl;

                return RedirectToAction("ForgotPasswordConfirmation");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null, string email = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Code = code
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _ownerManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _ownerManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
    }
}
