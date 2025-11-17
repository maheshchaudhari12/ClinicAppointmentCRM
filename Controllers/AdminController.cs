using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentCRM.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")] // ✅ Only Admin role can access
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdminController : Controller
    {
        private readonly ClinicDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ClinicDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new AdminDashboardViewModel
                {
                    TotalPatients = await _context.Patients.CountAsync(),
                    ActivePatients = await _context.Patients
                        .CountAsync(p => p.UserLogin.IsActive),

                    TotalDoctors = await _context.Doctors.CountAsync(),
                    ActiveDoctors = await _context.Doctors
                        .CountAsync(d => d.UserLogin.IsActive),

                    TotalReceptionists = await _context.Receptions.CountAsync(),
                    ActiveReceptionists = await _context.Receptions
                        .CountAsync(r => r.UserLogin.IsActive),

                    TodayAppointments = await _context.Appointments
                        .CountAsync(a => a.AppointmentDateTime.Date == DateTime.Today),

                    PendingAppointments = await _context.Appointments
                        .CountAsync(a => a.Status == "Pending"),

                    RecentPatients = await _context.Patients
                        .Include(p => p.UserLogin)
                        .OrderByDescending(p => p.PatientId)
                        .Take(5)
                        .Select(p => new PatientViewModel
                        {
                            PatientId = p.PatientId,
                            FullName = p.FullName,
                            Email = p.UserLogin.Email,
                            Phone = p.Phone,
                            IsActive = p.UserLogin.IsActive
                        })
                        .ToListAsync(),

                    RecentDoctors = await _context.Doctors
                        .Include(d => d.UserLogin)
                        .OrderByDescending(d => d.DoctorId)
                        .Take(5)
                        .Select(d => new DoctorViewModel
                        {
                            DoctorId = d.DoctorId,
                            Username = d.UserLogin.Username,
                            Email = d.UserLogin.Email,
                            Specialization = d.Specialization,
                            IsActive = d.UserLogin.IsActive
                        })
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["Error"] = "Error loading dashboard";
                return View(new AdminDashboardViewModel());
            }
        }
    }
}

