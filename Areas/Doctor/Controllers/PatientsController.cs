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
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PatientsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Doctor/Patients
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Find the doctor profile
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.AppUserId == user.Id);

            if (doctor == null) return NotFound("Doctor profile not found.");

            // Get distinct patients who have appointments with this doctor
            var patients = await _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.AppUser)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .Select(a => a.Patient)
                .Distinct()
                .ToListAsync();

            return View(patients);
        }
    }
}
