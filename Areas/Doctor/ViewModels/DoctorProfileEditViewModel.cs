using System.ComponentModel.DataAnnotations;

namespace MediCare.Areas.Doctor.ViewModels
{
    public class DoctorProfileEditViewModel
    {
        // From AppUser
        [Required, StringLength(50, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        // From Doctor
        [Display(Name = "Consultation Fee")]
        [Range(0, 10000, ErrorMessage = "Fee must be between 0 and 10,000.")]
        [DataType(DataType.Currency)]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Experience (Years)")]
        [Range(0, 60, ErrorMessage = "Experience must be between 0 and 60 years.")]
        public int? ExperienceYears { get; set; }

        [Display(Name = "Profile Bio")]
        [StringLength(500, ErrorMessage = "Bio must be 500 characters or fewer.")]
        public string? ProfileInfo { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(30)]
        [RegularExpression(@"^[0-9+\-()\s]*$", ErrorMessage = "Phone number format is invalid.")]
        public string? PhoneNumber { get; set; }
    }
}
