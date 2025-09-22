using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ List of Patients
        public async Task<IActionResult> Patients()
        {
            var patients = await _context.Patients
                .Include(p => p.AppUser) // join with AppUser
                .ToListAsync();

            return View(patients);
        }

        // ✅ List of Doctors
        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.AppUser)     // join with AppUser
                .Include(d => d.Specialty)  // join with Specialty
                .ToListAsync();

            return View(doctors);
        }
    }
}
