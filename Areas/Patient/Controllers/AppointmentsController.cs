using MediCare.Data;
using MediCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;


namespace MediCare.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "Patient")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ View all appointments for the logged-in patient
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.AppUserId == userId);

            if (patient == null)
            {
                TempData["Error"] = "Patient record not found.";
                return RedirectToAction("Index", "Dashboard", new { area = "Patient" });
            }
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.AppUser) 
                .Include(a => a.Doctor.Specialty)
                .Include(a => a.Payment)
                .Include(a => a.Feedbacks)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();


            return View(appointments);
        }

        // ✅ View detailed information about a single appointment
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.AppUserId == userId);

            if (patient == null)
            {
                TempData["Error"] = "Patient record not found.";
                return RedirectToAction("Index", "Dashboard", new { area = "Patient" });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.AppUser)
                .Include(a => a.Doctor.Specialty)
                .Include(a => a.Doctor.Clinic)
                .Include(a => a.Payment)
                .Include(a => a.Feedbacks)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.PatientId == patient.PatientId);

            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(appointment);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPrescription(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.AppUser)
                .Include(a => a.Patient).ThenInclude(p => p.AppUser)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null || string.IsNullOrEmpty(appointment.Prescription))
                return NotFound("Prescription not available.");

            // Generate PDF content
            using var stream = new MemoryStream();
            var doc = new iTextSharp.text.Document();
            var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, stream);
            doc.Open();

            doc.Add(new iTextSharp.text.Paragraph($"Prescription for {appointment.Patient.AppUser.FullName}", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 18, iTextSharp.text.Font.BOLD)));
            doc.Add(new iTextSharp.text.Paragraph($"Doctor: Dr. {appointment.Doctor.AppUser.FullName}"));
            doc.Add(new iTextSharp.text.Paragraph($"Date: {appointment.DateTime:dddd, dd MMM yyyy hh:mm tt}\n\n"));
            doc.Add(new iTextSharp.text.Paragraph("Prescription Details:"));
            doc.Add(new iTextSharp.text.Paragraph(appointment.Prescription));

            doc.Close();

            return File(stream.ToArray(), "application/pdf", $"Prescription_{appointment.AppointmentId}.pdf");
        }
        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int id)
        {
            var appointment = await _context.Appointments
    .Include(a => a.Doctor)
        .ThenInclude(d => d.AppUser)
    .Include(a => a.Doctor)
        .ThenInclude(d => d.Specialty)  
    .Include(a => a.Patient)
        .ThenInclude(p => p.AppUser)
    .Include(a => a.Payment)
    .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound("Appointment not found.");

            if (!string.Equals(appointment.Status?.Trim(), "Paid", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(appointment.Status?.Trim(), "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Receipt is only available after payment.");
            }

            using var stream = new MemoryStream();
            var doc = new iTextSharp.text.Document(PageSize.A4, 50, 50, 60, 60);
            var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, stream);
            doc.Open();

            // ✅ Define fonts
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, new BaseColor(0, 102, 204)); // MediCare blue
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            var grayFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.GRAY);

            // ✅ Header: MediCare Logo + Title
            var headerTable = new PdfPTable(2);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 70f, 30f });
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var logo = new Paragraph("🩺 MediCare Clinic", titleFont);
            headerTable.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, VerticalAlignment = Element.ALIGN_MIDDLE });

            var dateCell = new PdfPCell(new Phrase($"Date: {DateTime.Now:dd MMM yyyy}", grayFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            headerTable.AddCell(dateCell);

            doc.Add(headerTable);
            doc.Add(new Paragraph("\n"));

            // ✅ Receipt Title
            var title = new Paragraph("PAYMENT RECEIPT", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);
            doc.Add(new Paragraph("\n"));

            // ✅ Appointment Info
            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SpacingBefore = 10f;
            infoTable.SpacingAfter = 20f;
            infoTable.DefaultCell.Border = Rectangle.NO_BORDER;

            void AddInfoRow(string label, string value)
            {
                infoTable.AddCell(new PdfPCell(new Phrase(label, headerFont)) { Border = Rectangle.NO_BORDER });
                infoTable.AddCell(new PdfPCell(new Phrase(value ?? "-", normalFont)) { Border = Rectangle.NO_BORDER });
            }

            AddInfoRow("Patient Name:", appointment.Patient.AppUser.FullName);
            AddInfoRow("Doctor:", $"Dr. {appointment.Doctor.AppUser.FullName}");
            AddInfoRow("Specialty:", appointment.Doctor.Specialty?.Name);
            AddInfoRow("Appointment Date:", appointment.DateTime.ToString("dddd, dd MMM yyyy hh:mm tt"));
            AddInfoRow("Payment Method:", appointment.Payment?.Method);
            AddInfoRow("Transaction ID:", appointment.Payment?.TransactionId);

            doc.Add(infoTable);

            // ✅ Divider Line
            var line = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, new BaseColor(200, 200, 200), Element.ALIGN_CENTER, -2);
            doc.Add(new Chunk(line));
            doc.Add(new Paragraph("\n"));

            // ✅ Fee Details
            PdfPTable feeTable = new PdfPTable(2);
            feeTable.WidthPercentage = 60;
            feeTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            feeTable.SpacingBefore = 10f;

            feeTable.AddCell(new PdfPCell(new Phrase("Consultation Fee", normalFont)) { Border = Rectangle.NO_BORDER });
            feeTable.AddCell(new PdfPCell(new Phrase($"฿{appointment.Fee:N2}", normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

            feeTable.AddCell(new PdfPCell(new Phrase("Total Paid", headerFont)) { Border = Rectangle.TOP_BORDER });
            feeTable.AddCell(new PdfPCell(new Phrase($"฿{appointment.Fee:N2}", headerFont)) { Border = Rectangle.TOP_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(feeTable);
            doc.Add(new Paragraph("\n"));

            // ✅ Footer
            doc.Add(new Paragraph("Thank you for choosing MediCare!", grayFont) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new Paragraph("This is an electronically generated receipt — no signature required.", grayFont) { Alignment = Element.ALIGN_CENTER });

            doc.Close();

            return File(stream.ToArray(), "application/pdf", $"Receipt_{appointment.AppointmentId}.pdf");
        }

    }
}
