using System.ComponentModel.DataAnnotations;

namespace MediCare.Models
{
    public class Specialty
    {
        public int SpecialtyId { get; set; }

        [Required(ErrorMessage = "Specialty name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<Doctor>? Doctors { get; set; }
    }
}
