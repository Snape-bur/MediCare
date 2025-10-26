using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MediCare.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "Patient")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // 🚦 If wizard not finished, send to Step1
            if (!user.ProfileCompleted)
            {
                return RedirectToAction("Step1", "Registration", new { area = "Patient" });
            }

            // 🚦 Load patient with related data
            var patient = await _db.Patients
                .Include(p => p.AppUser)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.AppUser)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.Specialty)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Payment)
                .FirstOrDefaultAsync(p => p.AppUserId == user.Id);

            if (patient == null)
            {
                // Failsafe: if no patient record, restart registration
                return RedirectToAction("Step1", "Registration", new { area = "Patient" });
            }

            // ✅ Summary statistics
            ViewBag.TotalAppointments = patient.Appointments?.Count() ?? 0;
            ViewBag.CompletedAppointments = patient.Appointments?
                .Count(a => a.Status != null && a.Status.Trim().Equals("Completed", StringComparison.OrdinalIgnoreCase)) ?? 0;
            ViewBag.UpcomingAppointments = patient.Appointments?
                .Count(a => a.DateTime > DateTime.Now) ?? 0;

            // ✅ Next appointment
            var nextAppointment = patient.Appointments?
                .Where(a => a.DateTime > DateTime.Now)
                .OrderBy(a => a.DateTime)
                .FirstOrDefault();

            ViewBag.NextAppointment = nextAppointment;

            return View(patient);
        }

    }
}
