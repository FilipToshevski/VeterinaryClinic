using Microsoft.AspNetCore.Mvc;

namespace VeterinaryClinic.Controllers
{
    public class PrivacyController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ClinicName"] = "Veterinary Clinic";
            ViewData["ClinicAddress"] = "123 Clinic Street, Skopje, Macedonia";
            ViewData["ClinicEmail"] = "contact@veterinaryclinic.com";
            ViewData["ClinicPhone"] = "078 888 888";
            return View();
        }
    }
}
