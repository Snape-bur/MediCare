using System;
using System.ComponentModel.DataAnnotations;

namespace MediCare.Areas.Patient.Models
{
    public class Step1PersonalInfoVM
    {
        [Required, StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }


        [Required]
        public string Gender { get; set; }
    }
}
