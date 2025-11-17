using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicAppointmentCRM.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "Reception")] // ✅ Only Reception role can access
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReceptionController : BaseMvcController
    {
        private readonly ILogger<ReceptionController> _logger;
        private readonly ClinicDbContext _context;

        public ReceptionController(ILogger<ReceptionController> logger, ClinicDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Public reception info page
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        [AllowAnonymous] // ✅ Public access
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Reception Dashboard - Authenticated Reception only
        /// </summary>
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            var receptionId = GetCurrentReceptionId();
            if (receptionId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var reception = _context.Receptions
                .Include(r => r.UserLogin)
                .FirstOrDefault(r => r.ReceptionId == receptionId);

            ViewBag.ReceptionName = reception?.FullName;
            ViewBag.ReceptionId = receptionId;

            // Get statistics
            ViewBag.TodayAppointments = _context.Appointments
                .Count(a => a.AppointmentDateTime.Date == DateTime.Today);

            ViewBag.PendingAppointments = _context.Appointments
                .Count(a => a.Status == "Pending");

            _logger.LogInformation("Reception Dashboard accessed by Reception ID: {ReceptionId}", receptionId);
            return View();
        }

        /// <summary>
        /// Manage all appointments
        /// </summary>
        [HttpGet("Appointments")]
        public IActionResult Appointments()
        {
            var receptionId = GetCurrentReceptionId();
            if (receptionId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToList();

            return View(appointments);
        }

        /// <summary>
        /// Create new appointment (Reception can book for patients)
        /// </summary>
        [HttpGet("CreateAppointment")]
        public IActionResult CreateAppointment()
        {
            var receptionId = GetCurrentReceptionId();
            if (receptionId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Load patients and doctors for dropdowns
            ViewBag.Patients = _context.Patients
                .Include(p => p.UserLogin)
                .Where(p => p.UserLogin.IsActive)
                .ToList();

            ViewBag.Doctors = _context.Doctors
                .Include(d => d.UserLogin)
                .Where(d => d.UserLogin.IsActive)
                .ToList();

            return View();
        }

        /// <summary>
        /// Process appointment creation
        /// </summary>
        [HttpPost("CreateAppointment")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAppointment(Appointment model)
        {
            var receptionId = GetCurrentReceptionId();
            if (receptionId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            model.ReceptionId = receptionId;
            model.Status = "Confirmed";

            _context.Appointments.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Appointment created successfully!";
            return RedirectToAction("Appointments");
        }

        /// <summary>
        /// Update appointment status
        /// </summary>
        [HttpPost("UpdateAppointmentStatus")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAppointmentStatus(int appointmentId, string status)
        {
            var receptionId = GetCurrentReceptionId();
            if (receptionId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var appointment = _context.Appointments.Find(appointmentId);
            if (appointment != null)
            {
                appointment.Status = status;
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Appointment status updated!";
            }

            return RedirectToAction("Appointments");
        }

        /// <summary>
        /// View all patients
        /// </summary>
        [HttpGet("Patients")]
        public IActionResult Patients()
        {
            var receptionId = GetCurrentReceptionId();
            if (receptionId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var patients = _context.Patients
                .Include(p => p.UserLogin)
                .ToList();

            return View(patients);
        }

        /// <summary>
        /// View reception profile
        /// </summary>
        [HttpGet("Profile")]
        public IActionResult Profile()
        {
            var receptionId = GetCurrentReceptionId();
            if (receptionId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var reception = _context.Receptions
                .Include(r => r.UserLogin)
                .FirstOrDefault(r => r.ReceptionId == receptionId);

            if (reception == null)
            {
                return NotFound();
            }

            return View(reception);
        }

        /// <summary>
        /// Helper method to get current reception ID
        /// </summary>
        private int GetCurrentReceptionId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var reception = _context.Receptions.FirstOrDefault(r => r.UserId == userId);
                return reception?.ReceptionId ?? 0;
            }
            return 0;
        }
    }
}