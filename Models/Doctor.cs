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
        public string? AvailabilitySchedule { get; set; }
        public string? ProfileInfo { get; set; }

        // Navigation
        public ICollection<Appointment> Appointments { get; set; }
    }
}
