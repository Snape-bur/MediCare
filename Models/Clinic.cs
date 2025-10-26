using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediCare.Models
{
    public class Clinic
    {
        public int ClinicId { get; set; }

        [Required(ErrorMessage = "Clinic name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200)]
        public string Address { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        // Relationships
        public ICollection<Doctor>? Doctors { get; set; }
    }
}
