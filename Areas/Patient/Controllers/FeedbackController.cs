using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MediCare.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "Patient")]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -----------------------------
        // GET: Leave Feedback
        // -----------------------------
        public async Task<IActionResult> Create(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.AppUser)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null || appointment.Status != "Completed")
            {
                TempData["Error"] = "Feedback can only be given for completed appointments.";
                return RedirectToAction("Index", "Appointments", new { area = "Patient" });
            }

            // Pass IDs through ViewBag so the form can post them
            ViewBag.AppointmentId = appointmentId;
            ViewBag.DoctorId = appointment.DoctorId;
            ViewBag.PatientId = appointment.PatientId;

            return View();
        }

        // -----------------------------
        // POST: Submit Feedback
        // -----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int appointmentId, int doctorId, int patientId, int rating, string comments)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Please select a valid rating.";
                return RedirectToAction("Create", new { appointmentId });
            }

            // 🧩 Prevent duplicate feedback for same appointment
            var existingFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.AppointmentId == appointmentId);

            if (existingFeedback != null)
            {
                TempData["Error"] = "You have already submitted feedback for this appointment.";
                return RedirectToAction("Index", "Appointments", new { area = "Patient" });
            }

            var feedback = new Feedback
            {
                AppointmentId = appointmentId,
                DoctorId = doctorId,
                PatientId = patientId,
                Rating = rating,
                Comments = comments,
                CreatedAt = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Feedback submitted successfully!";
            return RedirectToAction("Index", "Appointments", new { area = "Patient" });
        }

    }
}
