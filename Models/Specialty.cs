using System.Numerics;

namespace MediCare.Models
{
    public class Specialty
    {
        public int SpecialtyId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        // Navigation
        public ICollection<Doctor> Doctors { get; set; }
    }
}
