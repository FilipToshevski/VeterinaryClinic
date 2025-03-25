using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VeterinaryClinic.Models;
using VeterinaryClinic.View_Models;

namespace VeterinaryClinic.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<Owner> _ownerManager;
        private readonly SignInManager<Owner> _signInManager;

        public AccountController(UserManager<Owner> ownerManager, SignInManager<Owner> signInManager)
        {
            _ownerManager = ownerManager;
            _signInManager = signInManager;
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
        public async Task<IActionResult> Profile()
        {
            return View();
        }

    }
}
