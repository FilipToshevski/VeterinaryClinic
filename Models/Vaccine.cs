namespace VeterinaryClinic.Models
{
    public class Vaccine
    {
        
        public string Name { get; set; }
        public List<Pets> Pets { get; set; } = new List<Pets>();
    }
}
