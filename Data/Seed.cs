using MediCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MediCare.Data
{
    public static class Seed
    {
        // -------------------------------
        // 1️⃣ Seed Roles
        // -------------------------------
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

        // -------------------------------
        // 2️⃣ Seed Admin
        // -------------------------------
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

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

                var result = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        // -------------------------------
        // 3️⃣ Seed Clinics
        // -------------------------------
        public static async Task SeedClinicsAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!context.Clinics.Any())
            {
                var clinics = new List<Clinic>
                {
                    new Clinic
                    {
                        ClinicId = 1,
                        Name = "CityCare Clinic",
                        Address = "123 Main St, Colombo",
                        PhoneNumber = "011-2345678",
                        Email = "info@citycare.lk"
                    },
                    new Clinic
                    {
                        ClinicId = 2,
                        Name = "HealthPlus Center",
                        Address = "88 Union Rd, Kandy",
                        PhoneNumber = "081-5678901",
                        Email = "contact@healthplus.lk"
                    },
                    new Clinic
                    {
                        ClinicId = 3,
                        Name = "Sunrise Medical",
                        Address = "45 Galle Rd, Negombo",
                        PhoneNumber = "031-7788990",
                        Email = "info@sunrisemed.lk"
                    }
                };

                context.Clinics.AddRange(clinics);
                await context.SaveChangesAsync();
            }
        }

        // -------------------------------
        // 4️⃣ Seed Doctors (linked to clinics)
        // -------------------------------
        public static async Task SeedDoctorsAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var doctors = new[]
            {
                new { Email = "dr.john@medicare.com", Name = "Dr. John Smith", SpecialtyId = 1, Fee = 500m, ClinicId = 1 },
                new { Email = "dr.sara@medicare.com", Name = "Dr. Sara Lee", SpecialtyId = 3, Fee = 800m, ClinicId = 2 },
                new { Email = "dr.kumar@medicare.com", Name = "Dr. Kumar", SpecialtyId = 4, Fee = 600m, ClinicId = 3 }
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

                    var result = await userManager.CreateAsync(user, "Doctor@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Doctor");

                        var doctorProfile = new Doctor
                        {
                            AppUserId = user.Id,
                            SpecialtyId = d.SpecialtyId,
                            ConsultationFee = d.Fee,
            
                            ProfileInfo = "Experienced healthcare professional",
                            ClinicId = d.ClinicId
                        };

                        context.Doctors.Add(doctorProfile);
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        // -------------------------------
        // 5️⃣ Seed Availabilities
        // -------------------------------
        public static async Task SeedAvailabilitiesAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!context.Availabilities.Any())
            {
                var doctors = context.Doctors.ToList();
                foreach (var doctor in doctors)
                {
                    context.Availabilities.AddRange(
                        new Availability
                        {
                            DoctorId = doctor.DoctorId,
                            Day = DayOfWeek.Monday,
                            StartTime = new TimeSpan(9, 0, 0),
                            EndTime = new TimeSpan(12, 0, 0)
                        },
                        new Availability
                        {
                            DoctorId = doctor.DoctorId,
                            Day = DayOfWeek.Wednesday,
                            StartTime = new TimeSpan(13, 0, 0),
                            EndTime = new TimeSpan(17, 0, 0)
                        }
                    );
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
