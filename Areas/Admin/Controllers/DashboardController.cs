using MediCare.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.DoctorCount = await _context.Doctors.CountAsync();
            ViewBag.PatientCount = await _context.Patients.CountAsync();
            ViewBag.ClinicCount = await _context.Clinics.CountAsync();
            ViewBag.AppointmentCount = await _context.Appointments.CountAsync();
            return View();
        }
    }
}
