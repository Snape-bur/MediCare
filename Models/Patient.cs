namespace MediCare.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        // Step 1 (always required)
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        // Step 2 (nullable until filled)
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        // Step 3 (nullable until filled)
        public string? MedicalHistory { get; set; }

        // Insurance info (optional)
        public string? InsuranceProvider { get; set; }
        public string? PolicyNumber { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }

        public ICollection<Appointment> Appointments { get; set; }

        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
