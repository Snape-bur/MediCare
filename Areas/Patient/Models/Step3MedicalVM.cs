using System.ComponentModel.DataAnnotations;

namespace MediCare.Areas.Patient.Models
{
    public class Step3MedicalVM
    {
        [Display(Name = "Medical History (optional)")]
        public string MedicalHistory { get; set; }
    }
}
