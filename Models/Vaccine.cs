namespace VeterinaryClinic.Models
{
    public class Vaccine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public ICollection<PetVaccine> PetVaccines { get; set; } = new List<PetVaccine>();
    }
}
