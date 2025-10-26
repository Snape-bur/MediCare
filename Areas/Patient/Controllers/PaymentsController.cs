using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MediCare.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "Patient")]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Show payment page
        public async Task<IActionResult> Index(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.AppUser)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound();

            var payment = new Payment
            {
                AppointmentId = appointmentId,
                Appointment = appointment,
                Amount = appointment.Fee
            };

            return View(payment);
        }

        // ✅ Mock payment confirmation
        [HttpPost]
        public async Task<IActionResult> Confirm(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.AppUser)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound();

            // Create payment record
            var payment = new Payment
            {
                AppointmentId = appointmentId,
                Amount = appointment.Fee,
                Status = "Paid",
                TransactionId = Guid.NewGuid().ToString().Substring(0, 8),
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            appointment.Status = "Paid";
            await _context.SaveChangesAsync();

            // ✅ Redirect to success page with payment details
            return RedirectToAction("Success", new { appointmentId, payment.TransactionId });
        }

        // ✅ Show success page
        public async Task<IActionResult> Success(int appointmentId, string transactionId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.AppUser)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound();

            ViewBag.TransactionId = transactionId;
            return View(appointment);
        }
    }
}
