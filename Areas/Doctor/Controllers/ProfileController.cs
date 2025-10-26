using MediCare.Areas.Doctor.ViewModels;
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
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Doctor/Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var doctor = await _context.Doctors
                .Include(d => d.AppUser)
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (doctor == null) return NotFound();

            var vm = new DoctorProfileEditViewModel
            {
                FullName = doctor.AppUser.FullName ?? string.Empty,
                ConsultationFee = doctor.ConsultationFee,
                ExperienceYears = doctor.ExperienceYears,
                ProfileInfo = doctor.ProfileInfo,
                PhoneNumber = doctor.PhoneNumber
            };

            return View(vm);
        }

        // POST: Doctor/Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // show validation errors
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var doctor = await _context.Doctors
                .Include(d => d.AppUser)
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (doctor == null) return NotFound();

            // Update allowed fields only
            doctor.AppUser.FullName = model.FullName.Trim();
            doctor.ConsultationFee = model.ConsultationFee;
            doctor.ExperienceYears = model.ExperienceYears;
            doctor.ProfileInfo = model.ProfileInfo?.Trim();
            doctor.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();

            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Profile updated successfully!";
            return RedirectToAction(nameof(Edit));
        }
    }
}
