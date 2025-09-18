namespace MediCare.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        // Relationships
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        // Payment details
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } // Card, Transfer, Cash
        public string Status { get; set; }        // Paid, Pending, Failed, Refunded
    }
}
