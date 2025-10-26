using MediCare.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using ClosedXML.Excel;
using System.IO;
using System;

namespace MediCare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Reports hub
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .ToListAsync();

            // ✅ KPI: Total Appointments
            ViewBag.TotalAppointments = appointments.Count;

            // ✅ KPI: Completed Count
            ViewBag.CompletedCount = appointments.Count(a => a.Status == "Completed");

            // ✅ KPI: Monthly Patients (Unique)
            var now = DateTime.Now;
            ViewBag.MonthlyPatients = appointments
                .Where(a => a.DateTime.Month == now.Month &&
                            a.DateTime.Year == now.Year)
                .Select(a => a.PatientId)
                .Distinct()
                .Count();

            // ✅ Monthly Chart Data
            var monthlyData = appointments
                .GroupBy(a => new { a.DateTime.Year, a.DateTime.Month })
                .Select(g => new
                {
                    Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                    Count = g.Count()
                })
                .OrderBy(g => g.Month)
                .ToList();

            ViewBag.Months = monthlyData.Select(m => m.Month).ToList();
            ViewBag.MonthCounts = monthlyData.Select(m => m.Count).ToList();

            // ✅ Appointment Status Doughnut Chart
            var statusData = appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.StatusLabels = statusData.Select(s => s.Status).ToList();
            ViewBag.StatusCounts = statusData.Select(s => s.Count).ToList();

            return View();
        }


        // ✅ Appointments Report
        public async Task<IActionResult> Appointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.AppUser)
                .Include(a => a.Patient).ThenInclude(p => p.AppUser)
                .ToListAsync();

            // 🗓️ Group by Month
            var monthlyData = appointments
                .GroupBy(a => new { a.DateTime.Year, a.DateTime.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Month:D2}-{g.Key.Year}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // 📊 Group by Status
            var statusData = appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.Months = monthlyData.Select(m => m.Month).ToList();
            ViewBag.MonthCounts = monthlyData.Select(m => m.Count).ToList();
            ViewBag.StatusLabels = statusData.Select(s => s.Status).ToList();
            ViewBag.StatusCounts = statusData.Select(s => s.Count).ToList();

            return View(appointments);
        }

        // ✅ Payments Report
        public async Task<IActionResult> Payments()
        {
            var payments = await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.AppUser)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(pt => pt.AppUser)
                .Where(p => p.Status == "Paid") // ✅ only show completed payments
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        // ✅ Patients Report
        public async Task<IActionResult> Patients()
        {
            var patients = await _context.Patients
                .Include(p => p.AppUser)
                .ToListAsync();

            int totalPatients = patients.Count;

            var monthlyRegistrations = patients
                .GroupBy(p => new { p.AppUser.CreatedAt.Month, p.AppUser.CreatedAt.Year })
                .Select(g => new
                {
                    Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                    Count = g.Count()
                })
                .OrderBy(g => g.Month)
                .ToList();

            var genderStats = patients
                .GroupBy(p => p.Gender ?? "Unknown")
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.TotalPatients = totalPatients;
            ViewBag.MonthlyData = monthlyRegistrations;
            ViewBag.GenderData = genderStats;

            return View(patients);
        }

        #region EXPORTS

        // ✅ Export Appointments to Excel (ClosedXML)
        [HttpGet]
        public async Task<IActionResult> ExportAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.AppUser)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialty)
                .Include(a => a.Doctor).ThenInclude(d => d.Clinic)
                .Include(a => a.Patient).ThenInclude(p => p.AppUser)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Appointments");

                // 🧱 Header Row
                ws.Cell(1, 1).Value = "Appointment ID";
                ws.Cell(1, 2).Value = "Doctor";
                ws.Cell(1, 3).Value = "Specialty";
                ws.Cell(1, 4).Value = "Clinic";
                ws.Cell(1, 5).Value = "Consultation Fee (฿)";
                ws.Cell(1, 6).Value = "Patient";
                ws.Cell(1, 7).Value = "Date & Time";
                ws.Cell(1, 8).Value = "Status";

                var headerRange = ws.Range("A1:H1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 🩺 Data Rows
                int row = 2;
                foreach (var a in appointments)
                {
                    ws.Cell(row, 1).Value = a.AppointmentId;
                    ws.Cell(row, 2).Value = $"Dr. {a.Doctor?.AppUser?.FullName ?? "N/A"}";
                    ws.Cell(row, 3).Value = a.Doctor?.Specialty?.Name ?? "N/A";
                    ws.Cell(row, 4).Value = a.Doctor?.Clinic?.Name ?? "N/A";
                    ws.Cell(row, 5).Value = a.Doctor?.ConsultationFee ?? 0;
                    ws.Cell(row, 6).Value = a.Patient?.AppUser?.FullName ?? "N/A";
                    ws.Cell(row, 7).Value = a.DateTime.ToString("dd MMM yyyy hh:mm tt");
                    ws.Cell(row, 8).Value = a.Status;
                    row++;
                }

                // 🧮 Summary Row
                var total = appointments.Count;
                var completed = appointments.Count(a => a.Status == "Completed");
                var cancelled = appointments.Count(a => a.Status == "Cancelled");

                ws.Cell(row + 1, 1).Value = "Total Appointments:";
                ws.Cell(row + 1, 2).Value = total;

                ws.Cell(row + 2, 1).Value = "Completed:";
                ws.Cell(row + 2, 2).Value = completed;

                ws.Cell(row + 3, 1).Value = "Cancelled:";
                ws.Cell(row + 3, 2).Value = cancelled;

                var summaryRange = ws.Range(row + 1, 1, row + 3, 2);
                summaryRange.Style.Font.Bold = true;
                summaryRange.Style.Fill.BackgroundColor = XLColor.AliceBlue;

                // 📏 Formatting
                ws.Column(5).Style.NumberFormat.Format = "#,##0.00";
                ws.Columns().AdjustToContents();

                var tableRange = ws.Range(1, 1, row - 1, 8);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Appointments_Report_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        // ✅ Export Payments to Excel
        [HttpGet]
        public async Task<IActionResult> ExportPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.AppUser)
                .Include(p => p.Appointment).ThenInclude(a => a.Patient).ThenInclude(p => p.AppUser)
                .Where(p => p.Status == "Paid")
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Payments");
                ws.Cell(1, 1).Value = "Payment ID";
                ws.Cell(1, 2).Value = "Appointment ID";
                ws.Cell(1, 3).Value = "Doctor";
                ws.Cell(1, 4).Value = "Patient";
                ws.Cell(1, 5).Value = "Amount (฿)";
                ws.Cell(1, 6).Value = "Method";
                ws.Cell(1, 7).Value = "Date";
                ws.Row(1).Style.Font.Bold = true;

                int row = 2;
                foreach (var p in payments)
                {
                    ws.Cell(row, 1).Value = p.PaymentId;
                    ws.Cell(row, 2).Value = p.AppointmentId;
                    ws.Cell(row, 3).Value = $"Dr. {p.Appointment.Doctor?.AppUser?.FullName}";
                    ws.Cell(row, 4).Value = p.Appointment.Patient?.AppUser?.FullName;
                    ws.Cell(row, 5).Value = p.Amount;
                    ws.Cell(row, 6).Value = p.Method;
                    ws.Cell(row, 7).Value = p.PaymentDate.ToString("dd MMM yyyy");
                    row++;
                }

                ws.Column(5).Style.NumberFormat.Format = "#,##0.00";
                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Payments_Report_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        // ✅ Export Patients to Excel
        [HttpGet]
        public async Task<IActionResult> ExportPatients()
        {
            var patients = await _context.Patients
                .Include(p => p.AppUser)
                .OrderBy(p => p.AppUser.FullName)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Patients");
                ws.Cell(1, 1).Value = "Patient ID";
                ws.Cell(1, 2).Value = "Full Name";
                ws.Cell(1, 3).Value = "Email";
                ws.Cell(1, 4).Value = "Gender";
                ws.Cell(1, 5).Value = "Address";
                ws.Cell(1, 6).Value = "Registered Date";
                ws.Row(1).Style.Font.Bold = true;

                int row = 2;
                foreach (var p in patients)
                {
                    ws.Cell(row, 1).Value = p.PatientId;
                    ws.Cell(row, 2).Value = p.AppUser.FullName;
                    ws.Cell(row, 3).Value = p.AppUser.Email;
                    ws.Cell(row, 4).Value = p.Gender;
                    ws.Cell(row, 5).Value = p.Address;
                    ws.Cell(row, 6).Value = p.AppUser.CreatedAt.ToString("dd MMM yyyy");
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Patients_Report_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        #endregion
    }
}
