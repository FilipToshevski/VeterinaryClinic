using System.ComponentModel.DataAnnotations;

namespace VeterinaryClinic.Models
{
    public class Pets
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public Owner Owner { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        [Required]
        [StringLength(50)]
        public string AnimalType { get; set; } // Fixed AnimalType property
        public ICollection<PetVaccine> PetVaccines { get; set; } = new List<PetVaccine>();

    }
}
