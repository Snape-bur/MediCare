using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCare.Data;
using MediCare.Models;

namespace MediCare.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SpecialtiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpecialtiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ INDEX: List all specialties
        public async Task<IActionResult> Index()
        {
            var specialties = await _context.Specialties.ToListAsync();
            return View(specialties);
        }

        // ✅ DETAILS: Show single specialty
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var specialty = await _context.Specialties.FirstOrDefaultAsync(m => m.SpecialtyId == id);
            if (specialty == null)
                return NotFound();

            return View(specialty);
        }

        // ✅ CREATE: Page
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Specialty specialty)
        {
            // Trim user input
            specialty.Name = specialty.Name?.Trim();
            specialty.Description = specialty.Description?.Trim();

            // Duplicate check
            var exists = await _context.Specialties.AnyAsync(s => s.Name == specialty.Name);
            if (exists)
            {
                TempData["Error"] = $"⚠️ A specialty named '{specialty.Name}' already exists.";
                return View(specialty);
            }

            // Model validation
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "⚠️ Please fill all required fields correctly.";
                return View(specialty);
            }

            _context.Add(specialty);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"✅ Specialty '{specialty.Name}' added successfully.";
            return RedirectToAction(nameof(Index));
        }


        // ✅ EDIT: Page
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null)
                return NotFound();

            return View(specialty);
        }

        // ✅ EDIT: Update existing specialty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpecialtyId,Name,Description")] Specialty specialty)
        {
            if (id != specialty.SpecialtyId)
                return NotFound();

            // 🔍 Prevent duplicate names on update
            var duplicate = await _context.Specialties
                .AnyAsync(s => s.Name == specialty.Name && s.SpecialtyId != id);
            if (duplicate)
            {
                TempData["Error"] = $"⚠️ Another specialty with the name '{specialty.Name}' already exists.";
                return View(specialty);
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "⚠️ Please check the fields and try again.";
                return View(specialty);
            }

            try
            {
                _context.Update(specialty);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"✅ Specialty '{specialty.Name}' updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpecialtyExists(specialty.SpecialtyId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ✅ DELETE: Confirm page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var specialty = await _context.Specialties.FirstOrDefaultAsync(m => m.SpecialtyId == id);
            if (specialty == null)
                return NotFound();

            return View(specialty);
        }

        // ✅ DELETE: Action
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty != null)
            {
                _context.Specialties.Remove(specialty);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"🗑️ Specialty '{specialty.Name}' deleted successfully.";
            }
            else
            {
                TempData["Error"] = "⚠️ Specialty not found or already deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ✅ Helper
        private bool SpecialtyExists(int id)
        {
            return _context.Specialties.Any(e => e.SpecialtyId == id);
        }
    }
}
