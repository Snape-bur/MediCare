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
    public class AvailabilityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AvailabilityController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Doctor/Availability/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.AppUserId == user.Id);
            if (doctor == null) return NotFound("Doctor profile not found.");

            return View(doctor);
        }

        // POST: Doctor/Availability/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int doctorId, string availabilitySchedule)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return NotFound();

            doctor.AvailabilitySchedule = availabilitySchedule;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard", new { area = "Doctor" });
        }
    }
}
