using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

        // ✅ Dashboard (User Management Overview)
        public async Task<IActionResult> Index()
        {
            ViewBag.DoctorCount = await _context.Doctors.CountAsync();
            ViewBag.PatientCount = await _context.Patients.CountAsync();
            ViewBag.ClinicCount = await _context.Clinics.CountAsync();

            return View();
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
                .Include(d => d.AppUser)    
                .Include(d => d.Specialty)   
                .Include(d => d.Clinic)     
                .ToListAsync();

            return View(doctors);
        }

    }
}
