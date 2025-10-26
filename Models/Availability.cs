namespace MediCare.Models
{
    public class Availability
    {
        public int AvailabilityId { get; set; }
        public int DoctorId { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Relationships
        public Doctor Doctor { get; set; }
    }

}
