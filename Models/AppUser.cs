using Microsoft.AspNetCore.Identity;

namespace MediCare.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }  // e.g. Male, Female, Other
        public string? ProfilePicture { get; set; }
    }
}
