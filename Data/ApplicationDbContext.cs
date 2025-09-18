using MediCare.Models; // <-- where your AppUser class is
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Fix decimal precision warnings
            builder.Entity<Doctor>()
                .Property(d => d.ConsultationFee)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // ✅ Fix multiple cascade paths (restrict instead of cascade)
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // ✅ Seed specialties
            builder.Entity<Specialty>().HasData(
                new Specialty { SpecialtyId = 1, Name = "General Medicine", Description = "Primary care and general health services" },
                new Specialty { SpecialtyId = 2, Name = "Pediatrics", Description = "Child health and development" },
                new Specialty { SpecialtyId = 3, Name = "Cardiology", Description = "Heart and cardiovascular care" },
                new Specialty { SpecialtyId = 4, Name = "Dermatology", Description = "Skin conditions and treatments" }
            );
        }
    }
}
