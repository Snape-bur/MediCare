using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MediCare.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")]
    public class AvailabilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AvailabilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: Doctor/Availabilities/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var doctor = await _context.Doctors
                .Include(d => d.Availabilities)
                .Include(d => d.AppUser)
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        // ✅ POST: Doctor/Availabilities/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(List<Availability> availabilities)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = await _context.Doctors
                .Include(d => d.Availabilities)
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (doctor == null)
                return Unauthorized();

            // Remove all old slots
            _context.Availabilities.RemoveRange(doctor.Availabilities);

            // Validation
            var seenSlots = new List<Availability>();
            var errors = new List<string>();

            foreach (var slot in availabilities)
            {
                if (!Enum.IsDefined(typeof(DayOfWeek), slot.Day) ||
                    slot.StartTime == TimeSpan.Zero ||
                    slot.EndTime == TimeSpan.Zero ||
                    slot.StartTime >= slot.EndTime)
                    continue;

                bool overlap = seenSlots.Any(s =>
                    s.Day == slot.Day &&
                    ((slot.StartTime >= s.StartTime && slot.StartTime < s.EndTime) ||
                     (slot.EndTime > s.StartTime && slot.EndTime <= s.EndTime) ||
                     (slot.StartTime <= s.StartTime && slot.EndTime >= s.EndTime)));

                if (overlap)
                {
                    errors.Add($"❌ Overlapping slot for {slot.Day} ({slot.StartTime:hh\\:mm}–{slot.EndTime:hh\\:mm})");
                }
                else
                {
                    slot.DoctorId = doctor.DoctorId;
                    seenSlots.Add(slot);
                    _context.Availabilities.Add(slot);
                }
            }

            if (errors.Any())
            {
                TempData["Error"] = string.Join("<br>", errors);
                return RedirectToAction(nameof(Edit));
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "✅ Availability updated successfully!";
            return RedirectToAction(nameof(Edit));
        }
    }
}
