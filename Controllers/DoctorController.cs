using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicAppointmentCRM.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "Doctor")] // ✅ Only Doctor role can access
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DoctorController : BaseMvcController
    {
        private readonly ILogger<DoctorController> _logger;
        private readonly ClinicDbContext _context;

        public DoctorController(ILogger<DoctorController> logger, ClinicDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Public doctor listing - No authentication required
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        [AllowAnonymous] // ✅ Override class-level authorization
        public IActionResult Index()
        {
            var doctors = _context.Doctors
                .Include(d => d.UserLogin)
                .Where(d => d.UserLogin.IsActive)
                .Select(d => new
                {
                    d.DoctorId,
                    d.Specialization,
                    d.AvailabilitySchedule,
                    d.Phone
                })
                .ToList();

            return View(doctors);
        }

        /// <summary>
        /// Doctor registration page - Public
        /// </summary>
        [HttpGet("Register")]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Doctor Dashboard - Authenticated Doctors only
        /// </summary>
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var doctor = _context.Doctors
                .Include(d => d.UserLogin)
                .FirstOrDefault(d => d.DoctorId == doctorId);

            ViewBag.DoctorName = doctor?.Specialization;
            ViewBag.DoctorId = doctorId;

            _logger.LogInformation("Doctor Dashboard accessed by Doctor ID: {DoctorId}", doctorId);
            return View();
        }

        /// <summary>
        /// View doctor's appointments
        /// </summary>
        [HttpGet("Appointments")]
        public IActionResult Appointments()
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Reception)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToList();

            return View(appointments);
        }

        /// <summary>
        /// View doctor's profile
        /// </summary>
        [HttpGet("Profile")]
        public IActionResult Profile()
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var doctor = _context.Doctors
                .Include(d => d.UserLogin)
                .FirstOrDefault(d => d.DoctorId == doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        /// <summary>
        /// Update doctor profile
        /// </summary>
        [HttpPost("UpdateProfile")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(Doctor model)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == 0 || doctorId != model.DoctorId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Update logic here
            return RedirectToAction("Profile");
        }

        /// <summary>
        /// View prescriptions written by doctor
        /// </summary>
        [HttpGet("Prescriptions")]
        public IActionResult Prescriptions()
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == 0)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var prescriptions = _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Appointment)
                .Where(p => p.DoctorId == doctorId)
                .OrderByDescending(p => p.IssuedDate)
                .ToList();

            return View(prescriptions);
        }

        /// <summary>
        /// Helper method to get current doctor ID
        /// </summary>
        private int GetCurrentDoctorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                return doctor?.DoctorId ?? 0;
            }
            return 0;
        }
    }
}