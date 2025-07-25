using Microsoft.AspNetCore.Identity;

namespace VeterinaryClinic.Models
{
    public class Owner : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int Age { get; set; }
        public List<Pets> Pets { get; set; } = new List<Pets>();


    }
}
