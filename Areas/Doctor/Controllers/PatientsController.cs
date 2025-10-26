using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

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

   
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.AppUserId == user.Id);

            if (doctor == null)
                return NotFound("Doctor profile not found.");

            var patients = await _context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.AppUser)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.Appointments)
                        .ThenInclude(a => a.Feedbacks) 
                .Select(a => a.Patient)
                .Distinct()
                .ToListAsync();

            return View(patients);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.AppUserId == user.Id);

            if (doctor == null)
                return NotFound("Doctor profile not found.");

            // Load patient and their appointments with this doctor only
            var patient = await _context.Patients
                .Include(p => p.AppUser)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .Where(p => p.Appointments.Any(a => a.DoctorId == doctor.DoctorId))
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
                return NotFound("Patient not found or not linked to this doctor.");

            return View(patient);
        }
    }
}
