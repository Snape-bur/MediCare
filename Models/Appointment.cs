namespace MediCare.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        // Relationships
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        // Appointment details
        public DateTime DateTime { get; set; }
        public string Status { get; set; } // Pending, Confirmed, Cancelled, etc.
        public string? Notes { get; set; }
        public string? Prescription { get; set; }

        // Navigation
        public Payment Payment { get; set; }
    }
}
