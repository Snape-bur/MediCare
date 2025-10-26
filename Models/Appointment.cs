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
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
        public string? Prescription { get; set; }

        public string? RescheduleReason { get; set; }
        public DateTime? RescheduledAt { get; set; }

        // Navigation
        public Payment Payment { get; set; }
        public decimal Fee { get; set; }
        public ICollection<Feedback>? Feedbacks { get; set; }
    }
}
