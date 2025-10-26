using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MediCare.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "Patient")]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ STEP 1: Search page
        public async Task<IActionResult> Index(int? specialtyId, int? clinicId)
        {
            ViewBag.Specialties = await _context.Specialties.ToListAsync();
            ViewBag.Clinics = await _context.Clinics.ToListAsync();

            var doctors = _context.Doctors
                .Include(d => d.AppUser)
                .Include(d => d.Specialty)
                .Include(d => d.Clinic)
                .AsQueryable();

            if (specialtyId.HasValue)
                doctors = doctors.Where(d => d.SpecialtyId == specialtyId);

            if (clinicId.HasValue)
                doctors = doctors.Where(d => d.ClinicId == clinicId);

            return View(await doctors.ToListAsync());
        }



        // ✅ STEP 2: Doctor details
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.AppUser)
                .Include(d => d.Specialty)
                .Include(d => d.Clinic)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
                return NotFound();

            ViewBag.Availabilities = await _context.Availabilities
                .Where(a => a.DoctorId == id)
                .OrderBy(a => a.Day)
                .ToListAsync();

            return View(doctor);
        }

        // ✅ STEP 3: Book appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int doctorId, DayOfWeek day, TimeSpan start, TimeSpan end)
        {
            try
            {
                // 1️⃣ Get logged-in user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user == null)
                {
                    TempData["Error"] = "User not found. Please log in again.";
                    return RedirectToAction("Index", "Booking");
                }

                // 2️⃣ Get the patient linked to this user
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.AppUserId == user.Id);
                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found. Please complete registration first.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Patient" });
                }

                // 3️⃣ Get doctor information
                var doctor = await _context.Doctors
                    .Include(d => d.AppUser)
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorId);
                if (doctor == null)
                {
                    TempData["Error"] = "Doctor not found.";
                    return RedirectToAction("Index", "Booking");
                }

                // 4️⃣ Validate the selected availability
                var availability = await _context.Availabilities
                    .FirstOrDefaultAsync(a => a.DoctorId == doctorId &&
                                              a.Day == day &&
                                              a.StartTime == start &&
                                              a.EndTime == end);
                if (availability == null)
                {
                    TempData["Error"] = "Selected time slot is no longer available.";
                    return RedirectToAction("Details", new { id = doctorId });
                }

                // 5️⃣ Calculate the correct next appointment DateTime
                DateTime now = DateTime.Now;
                DateTime appointmentDate = now.Date;

                // Move forward to the next matching weekday
                int daysUntil = ((int)day - (int)now.DayOfWeek + 7) % 7;
                appointmentDate = now.Date.AddDays(daysUntil).Add(start);

                // 🚨 Validation: cannot book in the past
                if (appointmentDate <= now)
                {
                    TempData["Error"] = "You cannot book an appointment in the past. Please select a future slot.";
                    return RedirectToAction("Details", new { id = doctorId });
                }

                // 6️⃣ Check if this time slot is already taken
                bool conflict = await _context.Appointments.AnyAsync(a =>
                    a.DoctorId == doctorId &&
                    a.DateTime == appointmentDate &&
                    a.Status != "Cancelled");

                if (conflict)
                {
                    TempData["Error"] = "That time slot is already booked. Please choose another.";
                    return RedirectToAction("Details", new { id = doctorId });
                }

                // 7️⃣ Create the appointment
                var appointment = new Appointment
                {
                    DoctorId = doctorId,
                    PatientId = patient.PatientId,
                    DateTime = appointmentDate,
                    Status = "Pending",
                    Fee = doctor.ConsultationFee,
                    Notes = $"Appointment requested for {appointmentDate:dddd, dd MMM yyyy} {start:hh\\:mm}-{end:hh\\:mm}"
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Message"] = "✅ Appointment booked successfully!";
                return RedirectToAction("Index", "Appointments", new { area = "Patient" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Booking Error: {ex.Message}");
                TempData["Error"] = "An unexpected error occurred while booking.";
                return RedirectToAction("Index", "Booking");
            }
        }




    }
}
