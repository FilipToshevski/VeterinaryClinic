
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc;

    namespace VeterinaryClinic.ViewModels
    {
        public class EditOwnerViewModel
        {
            public string Id { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "First name is required")]
            [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Date of birth is required")]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            
            
        }
    }

