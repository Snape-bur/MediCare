using System;
using System.ComponentModel.DataAnnotations;

namespace MediCare.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public string Method { get; set; } = "Mock";
        public string Status { get; set; } = "Pending";
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string? TransactionId { get; set; }
    }
}
