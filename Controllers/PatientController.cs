using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicAppointmentCRM.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "Patient")] // ✅ Only Patient role can access
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PatientController : BaseMvcController
    {
        private readonly ILogger<PatientController> _logger;
        private readonly ClinicDbContext _context;

        public PatientController(ILogger<PatientController> logger, ClinicDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Public patient info page
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        [AllowAnonymous] // ✅ Public access
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Patient registration page - Public
        /// </summary>
        [HttpGet("Register")]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Patient Dashboard - Authenticated Patients only
        /// </summary>
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            var patientId = GetCurrentPatientId();
            if (patientId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var patient = _context.Patients
                .Include(p => p.UserLogin)
                .FirstOrDefault(p => p.PatientId == patientId);

            ViewBag.PatientName = patient?.FullName;
            ViewBag.PatientId = patientId;

            _logger.LogInformation("Patient Dashboard accessed by Patient ID: {PatientId}", patientId);
            return View();
        }

        /// <summary>
        /// View patient's appointments
        /// </summary>
        [HttpGet("Appointments")]
        public IActionResult Appointments()
        {
            var patientId = GetCurrentPatientId();
            if (patientId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Reception)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToList();

            return View(appointments);
        }

        /// <summary>
        /// Book new appointment
        /// </summary>
        [HttpGet("BookAppointment")]
        public IActionResult BookAppointment()
        {
            var patientId = GetCurrentPatientId();
            if (patientId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Load doctors for dropdown
            ViewBag.Doctors = _context.Doctors
                .Include(d => d.UserLogin)
                .Where(d => d.UserLogin.IsActive)
                .ToList();

            return View();
        }

        /// <summary>
        /// Process appointment booking
        /// </summary>
        [HttpPost("BookAppointment")]
        [ValidateAntiForgeryToken]
        public IActionResult BookAppointment(Appointment model)
        {
            var patientId = GetCurrentPatientId();
            if (patientId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            model.PatientId = patientId;
            model.Status = "Pending";

            // Save appointment logic
            _context.Appointments.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Appointment booked successfully!";
            return RedirectToAction("Appointments");
        }

        /// <summary>
        /// View patient's profile
        /// </summary>
        [HttpGet("Profile")]
        public IActionResult Profile()
        {
            var patientId = GetCurrentPatientId();
            if (patientId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var patient = _context.Patients
                .Include(p => p.UserLogin)
                .FirstOrDefault(p => p.PatientId == patientId);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        /// <summary>
        /// Update patient profile
        /// </summary>
        [HttpPost("UpdateProfile")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(Patient model)
        {
            var patientId = GetCurrentPatientId();
            if (patientId == 0 || patientId != model.PatientId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Update logic here
            return RedirectToAction("Profile");
        }

        /// <summary>
        /// View patient's prescriptions
        /// </summary>
        [HttpGet("Prescriptions")]
        public IActionResult Prescriptions()
        {
            var patientId = GetCurrentPatientId();
            if (patientId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var prescriptions = _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Appointment)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.IssuedDate)
                .ToList();

            return View(prescriptions);
        }

        /// <summary>
        /// View medical history
        /// </summary>
        [HttpGet("MedicalHistory")]
        public IActionResult MedicalHistory()
        {
            var patientId = GetCurrentPatientId();
            if (patientId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Combine appointments and prescriptions
            var history = new
            {
                Appointments = _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patientId)
                    .OrderByDescending(a => a.AppointmentDateTime)
                    .ToList(),
                Prescriptions = _context.Prescriptions
                    .Include(p => p.Doctor)
                    .Where(p => p.PatientId == patientId)
                    .OrderByDescending(p => p.IssuedDate)
                    .ToList()
            };

            return View(history);
        }

        /// <summary>
        /// Helper method to get current patient ID
        /// </summary>
        private int GetCurrentPatientId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
                return patient?.PatientId ?? 0;
            }
            return 0;
        }
    }
}