using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCare.Data;
using MediCare.Models;

namespace MediCare.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClinicsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClinicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Clinics
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clinics.ToListAsync());
        }

        // GET: Admin/Clinics/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.ClinicId == id);
            if (clinic == null)
                return NotFound();

            return View(clinic);
        }

        // GET: Admin/Clinics/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Clinics/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClinicId,Name,Address,PhoneNumber,Email")] Clinic clinic)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clinic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(clinic);
        }

        // GET: Admin/Clinics/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
                return NotFound();

            return View(clinic);
        }

        // POST: Admin/Clinics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClinicId,Name,Address,PhoneNumber,Email")] Clinic clinic)
        {
            if (id != clinic.ClinicId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clinic);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Clinics.Any(e => e.ClinicId == id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(clinic);
        }

        // GET: Admin/Clinics/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.ClinicId == id);
            if (clinic == null)
                return NotFound();

            return View(clinic);
        }

        // POST: Admin/Clinics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic != null)
            {
                _context.Clinics.Remove(clinic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
