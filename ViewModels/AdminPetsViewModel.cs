using VeterinaryClinic.Models;

namespace VeterinaryClinic.ViewModels
{
    public class AdminPetsViewModel
    {
        public List<OwnerWithPets> OwnersWithPets { get; set; } = new List<OwnerWithPets>();

        public string SearchTerm { get; set; }
    }
    public class OwnerWithPets
    {
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public List<PetDetails> Pets { get; set; } = new List<PetDetails>();
    }

    public class PetDetails
    {
        public int PetId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string AnimalType { get; set; }
    }
}