using MediCare.Areas.Patient.Models;
using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace MediCare.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "Patient")] // users come here after Register + role assignment
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(ApplicationDbContext db, UserManager<AppUser> userManager, ILogger<RegistrationController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        // ===================== STEP 1 =====================
        [HttpGet]
        public IActionResult Step1() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Step1(Step1PersonalInfoVM vm)
        {
            _logger.LogInformation("==== Step1 POST fired ====");
            _logger.LogInformation(
                "Step1 POST received values: FirstName='{First}', LastName='{Last}', DOB='{DOB}', Gender='{Gender}'",
                vm.FirstName, vm.LastName, vm.DateOfBirth, vm.Gender);

            if (!ModelState.IsValid)
            {
                // 🔍 log all validation errors
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning("❌ Validation failed on {Field}: {Error}", entry.Key, error.ErrorMessage);
                    }
                }

                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User)
                       ?? (User.Identity?.Name != null
                           ? await _userManager.FindByEmailAsync(User.Identity.Name)
                           : null);

            if (user == null)
            {
                _logger.LogWarning("Step1 POST - User not found, redirecting to login");
                return RedirectToPage("/Account/Login",
                    new { area = "Identity", returnUrl = Url.Action("Step1", "Registration", new { area = "Patient" }) });
            }

            if (!vm.DateOfBirth.HasValue)
            {
                ModelState.AddModelError("DateOfBirth", "Date of Birth is required.");
                _logger.LogWarning("Step1 POST - DateOfBirth missing");
                return View(vm);
            }

            // ✅ Check if patient already exists
            var existingPatient = _db.Patients.FirstOrDefault(p => p.AppUserId == user.Id);
            if (existingPatient != null)
            {
                HttpContext.Session.SetInt32("PatientId", existingPatient.PatientId);
                _logger.LogInformation("Step1 POST - Existing patient found (ID={PatientId}) -> Redirect Step2", existingPatient.PatientId);
                return RedirectToAction("Step2");
            }

            // ✅ Create new patient
            var patient = new MediCare.Models.Patient
            {
                AppUserId = user.Id,
                FirstName = vm.FirstName!.Trim(),
                LastName = vm.LastName!.Trim(),
                DateOfBirth = vm.DateOfBirth.Value,
                Gender = vm.Gender,
                PhoneNumber = null,
                Address = null,
                MedicalHistory = ""
            };

            try
            {
                _db.Patients.Add(patient);

                // Sync AppUser
                user.FullName = $"{vm.FirstName} {vm.LastName}";
                user.DateOfBirth = vm.DateOfBirth;
                user.Gender = vm.Gender;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Step1 POST - Saving patient {First} {Last}", patient.FirstName, patient.LastName);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Step1 POST - SaveChanges finished -> PatientId={PatientId}", patient.PatientId);

                HttpContext.Session.SetInt32("PatientId", patient.PatientId);

                return RedirectToAction("Step2");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Step1 POST - Error saving patient");
                ModelState.AddModelError("", "An error occurred while saving your data. Please contact support.");
                return View(vm);
            }
        }



        // ===================== STEP 2 =====================
        [HttpGet]
        public IActionResult Step2()
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Step1");

            _logger.LogInformation("Step2 GET: Retrieved PatientId={PatientId} from Session", patientId);
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Step2(Step2ContactVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Step1");

            var patient = await _db.Patients.FindAsync(patientId);
            if (patient == null) return RedirectToAction("Step1");

            patient.PhoneNumber = vm.PhoneNumber;
            patient.Address = vm.Address;

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.PhoneNumber = vm.PhoneNumber;
                user.Address = vm.Address;
                await _userManager.UpdateAsync(user);
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Step2 POST: Updated contact info for PatientId={PatientId}", patient.PatientId);

            return RedirectToAction("Step3");
        }

        // ===================== STEP 3 =====================
        [HttpGet]
        public IActionResult Step3()
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Step1");

            _logger.LogInformation("Step3 GET: Retrieved PatientId={PatientId} from Session", patientId);
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Step3(Step3MedicalVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Step1");

            var patient = await _db.Patients.FindAsync(patientId);
            if (patient == null) return RedirectToAction("Step1");

            patient.MedicalHistory = vm.MedicalHistory;
            await _db.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.ProfileCompleted = true;
                await _userManager.UpdateAsync(user);
            }

            HttpContext.Session.Remove("PatientId"); // clean up

            return RedirectToAction("Index", "Dashboard", new { area = "Patient" }); // ✅ go to Dashboard
        }

    }
}