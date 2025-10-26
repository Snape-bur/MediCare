using System.Diagnostics;
using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Specialties = _context.Specialties.ToList();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Search(int? specialtyId)
        {
            if (!specialtyId.HasValue)
                return RedirectToAction("Index");

            var doctors = await _context.Doctors
                .Include(d => d.AppUser)
                .Include(d => d.Specialty)
                .Include(d => d.Clinic)
                .Where(d => d.SpecialtyId == specialtyId)
                .ToListAsync();

            ViewBag.SpecialtyName = doctors.FirstOrDefault()?.Specialty?.Name
                                    ?? "Selected Specialty";

            return View(doctors);
        }

    }
}
