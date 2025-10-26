using System;
using System.ComponentModel.DataAnnotations;

namespace MediCare.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Appointment Appointment { get; set; }
        public Doctor Doctor { get; set; }
        public Patient Patient { get; set; }
    }
}
