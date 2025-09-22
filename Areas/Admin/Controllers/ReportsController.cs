using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        public IActionResult Appointments()
        {
            return View();
        }

        public IActionResult Payments()
        {
            return View();
        }

        public IActionResult Patients()
        {
            return View();
        }
    }
}
