using System.ComponentModel.DataAnnotations;

namespace MediCare.Areas.Patient.Models
{
    public class Step2ContactVM
    {
        [Required, Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }
    }
}
