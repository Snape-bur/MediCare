using MediCare.Models;
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

        // ✅ DbSets
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Availability> Availabilities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Decimal precision
            builder.Entity<Doctor>()
                .Property(d => d.ConsultationFee)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // ✅ Appointment relationships
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

            // ✅ Clinic relationship
            builder.Entity<Doctor>()
                .HasOne(d => d.Clinic)
                .WithMany(c => c.Doctors)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Specialty relationship (RECONNECTED FK)
            builder.Entity<Doctor>()
                .HasOne(d => d.Specialty)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Feedback relationships
            builder.Entity<Feedback>()
                .HasOne(f => f.Doctor)
                .WithMany()
                .HasForeignKey(f => f.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Feedback>()
      .HasOne(f => f.Patient)
      .WithMany(p => p.Feedbacks)
      .HasForeignKey(f => f.PatientId)
      .OnDelete(DeleteBehavior.Restrict);


            // ✅ Prescription → Appointment
            builder.Entity<Prescription>()
                .HasOne(p => p.Appointment)
                .WithOne()
                .HasForeignKey<Prescription>(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Availability → Doctor
            builder.Entity<Availability>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Availabilities)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Appointment → Feedback (One-to-Many)
            builder.Entity<Appointment>()
                .HasMany(a => a.Feedbacks)
                .WithOne(f => f.Appointment)
                .HasForeignKey(f => f.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Seed Specialties
            builder.Entity<Specialty>().HasData(
                new Specialty { SpecialtyId = 1, Name = "General Medicine", Description = "Primary care and general health services" },
                new Specialty { SpecialtyId = 2, Name = "Pediatrics", Description = "Child health and development" },
                new Specialty { SpecialtyId = 3, Name = "Cardiology", Description = "Heart and cardiovascular care" },
                new Specialty { SpecialtyId = 4, Name = "Dermatology", Description = "Skin conditions and treatments" },
                new Specialty { SpecialtyId = 5, Name = "Orthopedics", Description = "Bone, joint, and muscle care" },
                new Specialty { SpecialtyId = 6, Name = "Gynecology", Description = "Women’s reproductive health, pregnancy, and fertility management" },
                new Specialty { SpecialtyId = 7, Name = "Neurology", Description = "Brain and nervous system disorders" }
            );
        }
    }
}
