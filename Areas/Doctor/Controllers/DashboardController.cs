using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MediCare.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Get logged-in doctor user
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Unauthorized();

            // Find Doctor profile linked to AppUser
            var doctor = await _context.Doctors
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.AppUserId == user.Id);

            if (doctor == null) return NotFound("Doctor profile not found.");

            // Count upcoming appointments
            var upcomingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId && a.DateTime >= DateTime.Now)
                .CountAsync();

            ViewData["DoctorName"] = user.FullName;
            ViewData["Specialty"] = doctor.Specialty?.Name;
            ViewData["UpcomingAppointments"] = upcomingAppointments;

            return View();
        }
    }
}
