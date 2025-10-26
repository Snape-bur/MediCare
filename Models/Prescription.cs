namespace MediCare.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        public int AppointmentId { get; set; }
        public string Notes { get; set; }
        public string Medicines { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relationships
        public Appointment Appointment { get; set; }
    }

}
