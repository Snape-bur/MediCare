using MediCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MediCare.Data
{
    public static class Seed
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "Admin", "Doctor", "Patient" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // Create default admin if not exists
            var adminEmail = "admin@medicare.lk";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin@123"); // Strong password required
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        // ✅ Seed sample doctors
        public static async Task SeedDoctorsAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Example doctor accounts
            var doctors = new[]
            {
            new { Email = "dr.john@medicare.com", Name = "Dr. John Smith", SpecialtyId = 1, Fee = 500m },
            new { Email = "dr.sara@medicare.com", Name = "Dr. Sara Lee", SpecialtyId = 3, Fee = 800m },
            new { Email = "dr.kumar@medicare.com", Name = "Dr. Kumar", SpecialtyId = 4, Fee = 600m }
        };

            foreach (var d in doctors)
            {
                var user = await userManager.FindByEmailAsync(d.Email);
                if (user == null)
                {
                    user = new AppUser
                    {
                        UserName = d.Email,
                        Email = d.Email,
                        FullName = d.Name,
                        Address = "Clinic Colombo",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "Doctor@123"); // default password
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Doctor");

                        // Create Doctor profile
                        var doctorProfile = new Doctor
                        {
                            AppUserId = user.Id,
                            SpecialtyId = d.SpecialtyId,
                            ConsultationFee = d.Fee,
                            AvailabilitySchedule = "Mon-Fri 9AM-5PM",
                            ProfileInfo = "Experienced healthcare professional"
                        };

                        context.Doctors.Add(doctorProfile);
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
