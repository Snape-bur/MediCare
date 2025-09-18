namespace MediCare.Models
{
    public class Patient
    {
        public int PatientId { get; set; }

        // Relationships
        public string AppUserId { get; set; }   // FK → AppUser
        public AppUser AppUser { get; set; }

        // Patient-specific fields
        public string? MedicalHistory { get; set; }
        public string? InsuranceDetails { get; set; }

        // Navigation
        public ICollection<Appointment> Appointments { get; set; }
    }
}
