using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Doctor/Appointments
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.AppUserId == user.Id);
            if (doctor == null) return NotFound("Doctor profile not found.");

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.AppUser)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderBy(a => a.DateTime)
                .ToListAsync();

            return View(appointments);
        }

        // POST: Confirm Appointment
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Confirmed";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Cancel Appointment
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET + POST: Reschedule Appointment
        [HttpGet]
        public async Task<IActionResult> Reschedule(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime newDateTime)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.DateTime = newDateTime;
            appointment.Status = "Rescheduled";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: Doctor/Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.AppUser)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialty)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: Save Notes & Prescription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, string notes, string prescription)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Notes = notes;
            appointment.Prescription = prescription;
            appointment.Status = "Completed";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
