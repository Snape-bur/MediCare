using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MediCare.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ View all appointments for logged-in doctor
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction("Index", "Dashboard", new { area = "Doctor" });
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.AppUser)
                .Include(a => a.Payment)
                .Include(a => a.Feedbacks) 
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderBy(a => a.DateTime)
                .ToListAsync();


            return View(appointments);
        }

        // ✅ Confirm appointment
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            if (appointment.Status != "Pending")
            {
                TempData["Error"] = "Only pending appointments can be confirmed.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = "Confirmed";
            await _context.SaveChangesAsync();

            TempData["Message"] = "Appointment confirmed successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditNotes(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.AppUser)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNotes(int id, [Bind("Notes,Prescription")] Appointment updated)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            appointment.Notes = updated.Notes;
            appointment.Prescription = updated.Prescription;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Consultation notes and prescription updated successfully.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Reschedule(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
            {
                TempData["Error"] = "You cannot reschedule a completed or cancelled appointment.";
                return RedirectToAction(nameof(Index));
            }
            var availabilities = await _context.Availabilities
                .Where(a => a.DoctorId == appointment.DoctorId)
                .ToListAsync();

            ViewBag.Availabilities = availabilities;
            return View(appointment);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime newDateTime, string? reason)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            // 🚫 Prevent rescheduling completed or cancelled appointments
            if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
            {
                TempData["Error"] = "You cannot reschedule a completed or cancelled appointment.";
                return RedirectToAction(nameof(Index));
            }

      
            appointment.DateTime = newDateTime;
            appointment.RescheduleReason = reason;
            appointment.RescheduledAt = DateTime.UtcNow;

            appointment.Status = "Pending";

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Appointment rescheduled successfully. Awaiting patient confirmation.";
            return RedirectToAction(nameof(Index));
        }




        // ✅ Mark appointment as Completed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            // Load and validate
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            // Only allow when current status is Paid (case/space tolerant)
            var isPaid = string.Equals(appt.Status?.Trim(), "Paid", StringComparison.OrdinalIgnoreCase);
            if (!isPaid)
            {
                TempData["Error"] = "Appointment must be paid before marking as completed.";
                return RedirectToAction(nameof(Index));
            }

            // Update & persist
            appt.Status = "Completed";
            _context.Appointments.Update(appt);   // ensure tracked & marked modified
            await _context.SaveChangesAsync();

            TempData["Message"] = "Appointment marked as completed successfully.";
            return RedirectToAction(nameof(Index));
        }


        // ✅ Cancel appointment
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            if (appointment.Status == "Paid" || appointment.Status == "Completed")
            {
                TempData["Error"] = "You cannot cancel a paid or completed appointment.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["Message"] = "Appointment cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (doctor == null)
                return RedirectToAction("Index", "Dashboard", new { area = "Doctor" });

            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.AppUser)
                .Include(a => a.Feedbacks)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.DoctorId == doctor.DoctorId);

            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(appointment);
        }

    }
}
