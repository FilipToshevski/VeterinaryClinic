namespace VeterinaryClinic.ViewModels
{
    public class VaccineDisplayViewModel
    {
        public int Id { get; set; }  // Vaccine ID
        public string Name { get; set; } = string.Empty;
        public DateTime DateAdministered { get; set; }
    }
}
