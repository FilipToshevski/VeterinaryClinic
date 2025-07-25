using System.ComponentModel.DataAnnotations;


namespace VeterinaryClinic.Models
{
    public class PetVaccine
    {
        public int PetId { get; set; }
        public Pets? Pet { get; set; }
        public int VaccineId { get; set; }
        public Vaccine? Vaccine { get; set; }
        public DateTime DateAdministered { get; set; } = DateTime.UtcNow;
    }
}