using VeterinaryClinic.Models;

namespace VeterinaryClinic.ViewModels
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? CalculatedAge { get; set; }

       
        public List<Pet> Pets { get; set; } = new List<Pet>();
    }
}
