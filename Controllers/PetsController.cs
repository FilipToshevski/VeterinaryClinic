using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryClinic.Data;
using VeterinaryClinic.Models;
using VeterinaryClinic.ViewModels;

namespace VeterinaryClinic.Controllers
{
    public class PetsController : Controller
    {
        private readonly UserManager<Owner> _userManager;
        private readonly VeterinaryClinicDb _context;

        public PetsController(UserManager<Owner> userManager, VeterinaryClinicDb context)
        {
            _userManager = userManager;
            _context = context;
        }

      
    }
}
