using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoctorModel = MediCare.Models.Doctor;

namespace MediCare.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DoctorsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // -------------------------------------------------------------
        // 🩺 GET: Admin/Doctors
        // -------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var doctors = _context.Doctors
                .Include(d => d.AppUser)
                .Include(d => d.Specialty)
                .Include(d => d.Clinic)
                .Include(d => d.Availabilities); // ✅ include availability list if you show it later

            return View(await doctors.ToListAsync());
        }

        // -------------------------------------------------------------
        // 🆕 GET: Admin/Doctors/Create
        // -------------------------------------------------------------
        public IActionResult Create()
        {
            ViewData["SpecialtyId"] = new SelectList(_context.Specialties, "SpecialtyId", "Name");
            ViewData["ClinicId"] = new SelectList(_context.Clinics, "ClinicId", "Name");
            return View();
        }

        // -------------------------------------------------------------
        // 🧑‍⚕️ POST: Admin/Doctors/Create
        // -------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string email,
            string fullName,
            string password,
            int specialtyId,
            int? clinicId,
            decimal consultationFee,
            string? qualification,
            string? phoneNumber,
            int? experienceYears,
            string? profileInfo)
        {
            if (ModelState.IsValid)
            {
                // 1️⃣ Create AppUser
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    PhoneNumber = phoneNumber
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    ViewData["SpecialtyId"] = new SelectList(_context.Specialties, "SpecialtyId", "Name", specialtyId);
                    ViewData["ClinicId"] = new SelectList(_context.Clinics, "ClinicId", "Name", clinicId);
                    return View();
                }

                // 2️⃣ Assign Doctor role
                await _userManager.AddToRoleAsync(user, "Doctor");

                // 3️⃣ Create Doctor record
                var doctor = new DoctorModel
                {
                    AppUserId = user.Id,
                    SpecialtyId = specialtyId,
                    ClinicId = clinicId,
                    ConsultationFee = consultationFee,
                    Qualification = qualification,
                    PhoneNumber = phoneNumber,
                    ExperienceYears = experienceYears,
                    ProfileInfo = profileInfo
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                // 4️⃣ (Optional) Redirect admin to Availability management for this doctor
                return RedirectToAction(nameof(Index));
            }

            ViewData["SpecialtyId"] = new SelectList(_context.Specialties, "SpecialtyId", "Name", specialtyId);
            ViewData["ClinicId"] = new SelectList(_context.Clinics, "ClinicId", "Name", clinicId);
            return View();
        }

        // -------------------------------------------------------------
        // 👁️ GET: Admin/Doctors/Details/5
        // -------------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.AppUser)
                .Include(d => d.Specialty)
                .Include(d => d.Clinic)
                .Include(d => d.Availabilities)
                .FirstOrDefaultAsync(m => m.DoctorId == id);

            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        // -------------------------------------------------------------
        // ✏️ GET: Admin/Doctors/Edit/5
        // -------------------------------------------------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.AppUser)
                .Include(d => d.Specialty)
                .Include(d => d.Clinic)
                .Include(d => d.Availabilities) // ✅ Add this line
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
                return NotFound();

            ViewData["SpecialtyId"] = new SelectList(_context.Specialties, "SpecialtyId", "Name", doctor.SpecialtyId);
            ViewData["ClinicId"] = new SelectList(_context.Clinics, "ClinicId", "Name", doctor.ClinicId);

            return View(doctor);
        }


        // -------------------------------------------------------------
        // ✏️ POST: Admin/Doctors/Edit/5
        // -------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            int specialtyId,
            int? clinicId,
            decimal consultationFee,
            string? qualification,
            string? phoneNumber,
            int? experienceYears,
            string? profileInfo)
        {
            var doctor = await _context.Doctors
                .Include(d => d.AppUser)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null) return NotFound();

            if (ModelState.IsValid)
            {
                doctor.SpecialtyId = specialtyId;
                doctor.ClinicId = clinicId;
                doctor.ConsultationFee = consultationFee;
                doctor.Qualification = qualification;
                doctor.PhoneNumber = phoneNumber;
                doctor.ExperienceYears = experienceYears;
                doctor.ProfileInfo = profileInfo;

                _context.Update(doctor);

                // update AppUser phone if needed
                if (doctor.AppUser != null && !string.IsNullOrEmpty(phoneNumber))
                {
                    doctor.AppUser.PhoneNumber = phoneNumber;
                    _context.Update(doctor.AppUser);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SpecialtyId"] = new SelectList(_context.Specialties, "SpecialtyId", "Name", specialtyId);
            ViewData["ClinicId"] = new SelectList(_context.Clinics, "ClinicId", "Name", clinicId);

            return View(doctor);
        }

        // -------------------------------------------------------------
        // 🗑️ GET: Admin/Doctors/Delete/5
        // -------------------------------------------------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.AppUser)
                .Include(d => d.Specialty)
                .Include(d => d.Clinic)
                .FirstOrDefaultAsync(m => m.DoctorId == id);

            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // -------------------------------------------------------------
        // 🗑️ POST: Admin/Doctors/Delete/5
        // -------------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
                _context.Doctors.Remove(doctor);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }
    }
}
