using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AvailabilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AvailabilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ List all doctors and their availability summary
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors
                .Include(d => d.AppUser)
                .Include(d => d.Specialty)
                .Include(d => d.Availabilities)
                .OrderBy(d => d.AppUser.FullName)
                .ToListAsync();

            return View(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // clear tracked entities
            _context.ChangeTracker.Clear();

            var doctor = await _context.Doctors
                .Include(d => d.Availabilities)
                .Include(d => d.AppUser)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null) return NotFound();

            return View(doctor);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, List<Availability> availabilities)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Availabilities)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
                return NotFound();

            // 🔹 Remove all old slots
            _context.Availabilities.RemoveRange(doctor.Availabilities);

            // 🔹 Validate duplicates or overlaps
            var seenSlots = new List<Availability>();
            var errorMessages = new List<string>();

            foreach (var slot in availabilities)
            {
                // Skip invalid entries
                if (!Enum.IsDefined(typeof(DayOfWeek), slot.Day) ||
                    slot.StartTime == TimeSpan.Zero ||
                    slot.EndTime == TimeSpan.Zero ||
                    slot.StartTime >= slot.EndTime)
                    continue;

                // Check for same-day duplicates or overlaps
                var overlap = seenSlots.Any(s =>
                    s.Day == slot.Day &&
                    ((slot.StartTime >= s.StartTime && slot.StartTime < s.EndTime) ||
                     (slot.EndTime > s.StartTime && slot.EndTime <= s.EndTime) ||
                     (slot.StartTime <= s.StartTime && slot.EndTime >= s.EndTime)));

                if (overlap)
                {
                    errorMessages.Add($"❌ Overlapping or duplicate slot detected for {slot.Day} ({slot.StartTime:hh\\:mm}–{slot.EndTime:hh\\:mm})");
                }
                else
                {
                    // Valid → add to both memory & DB
                    slot.DoctorId = id;
                    seenSlots.Add(slot);
                    _context.Availabilities.Add(slot);
                }
            }

            // 🔹 Handle errors
            if (errorMessages.Any())
            {
                TempData["Error"] = string.Join("<br>", errorMessages);
                return RedirectToAction(nameof(Edit), new { id });
            }

            // 🔹 Save valid data
            await _context.SaveChangesAsync();
            TempData["Message"] = "✅ Availability updated successfully!";
            return RedirectToAction(nameof(Edit), new { id });
        }


    }
}
