using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Include doctor with related data
            var doctor = await _context.Doctors
                .Include(d => d.Specialty)
                .Include(d => d.Availabilities)  // 👈 include availability list
                .FirstOrDefaultAsync(d => d.AppUserId == user.Id);

            if (doctor == null) return NotFound("Doctor profile not found.");

            // Count upcoming appointments
            var upcomingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId && a.DateTime >= DateTime.Now)
                .CountAsync();

            // Count unique patients
            var patientCount = await _context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            // Get next appointment
            var nextAppointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.AppUser)
                .Where(a => a.DoctorId == doctor.DoctorId && a.DateTime >= DateTime.Now)
                .OrderBy(a => a.DateTime)
                .Select(a => new
                {
                    a.DateTime,
                    a.Status,
                    PatientName = a.Patient.AppUser.FullName
                })
                .FirstOrDefaultAsync();

            // ✅ Today’s availability logic
            var today = DateTime.Today.DayOfWeek;
            var todayAvailability = doctor.Availabilities
                .FirstOrDefault(a => a.Day == today);

            string todaySchedule;
            if (todayAvailability != null)
            {
                todaySchedule = $"{todayAvailability.StartTime:hh\\:mm} - {todayAvailability.EndTime:hh\\:mm}";
            }
            else
            {
                todaySchedule = "Not available today";
            }

            // ✅ Pass data to view
            ViewData["DoctorName"] = user.FullName;
            ViewData["UpcomingAppointments"] = upcomingAppointments;
            ViewData["PatientCount"] = patientCount;
            ViewData["TodaySchedule"] = todaySchedule;
            ViewBag.NextAppointment = nextAppointment;

            return View();
        }
    }
}
