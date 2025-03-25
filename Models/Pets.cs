namespace VeterinaryClinic.Models
{
    public class Pets
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        
        public Owner Owner { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<Vaccine> Vaccine { get; set; } = new List<Vaccine>();

    }
}
