namespace MediCare.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }

        // Relationships
        public string AppUserId { get; set; }   // FK → AppUser
        public AppUser AppUser { get; set; }

        public int SpecialtyId { get; set; }    // FK → Specialty
        public Specialty Specialty { get; set; }

        // Doctor-specific fields
        public decimal ConsultationFee { get; set; }
        public string? ProfileInfo { get; set; }

        public int? ExperienceYears { get; set; }  // Optional
        public string? Qualification { get; set; } // e.g. MBBS, MD
        public string? PhoneNumber { get; set; }   // Contact number

        // Navigation
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Availability> Availabilities { get; set; }  

        public int? ClinicId { get; set; }
        public Clinic? Clinic { get; set; }
    }
}
