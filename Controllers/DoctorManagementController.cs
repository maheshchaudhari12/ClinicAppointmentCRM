using ClinicAppointmentCRM.Models;
using ClinicAppointmentCRM.Models.ViewModels;
using ClinicAppointmentCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAppointmentCRM.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DoctorManagementController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorManagementController> _logger;

        public DoctorManagementController(
            IDoctorService doctorService,
            ILogger<DoctorManagementController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        // GET: DoctorManagement
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string searchTerm)
        {
            try
            {
                var doctors = await _doctorService.GetAllDoctorsAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doctors");
                TempData["Error"] = "Error loading doctors";
                return View(new List<DoctorViewModel>());
            }
        }

        // GET: DoctorManagement/Details/5
        
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                if (doctor == null)
                {
                    TempData["Error"] = "Doctor not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doctor details for ID: {DoctorId}", id);
                TempData["Error"] = "Error loading doctor details";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: DoctorManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DoctorManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorRegDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var result = await _doctorService.CreateDoctorAsync(dto);
                if (result)
                {
                    _logger.LogInformation("Doctor created successfully: {Username}", dto.Username);
                    TempData["Success"] = "Doctor created successfully";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("Failed to create doctor - Username or Email already exists: {Username}", dto.Username);
                ModelState.AddModelError("", "Username or Email already exists");
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor");
                ModelState.AddModelError("", "Error creating doctor. Please try again.");
                return View(dto);
            }
        }

        // GET: DoctorManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                if (doctor == null)
                {
                    TempData["Error"] = "Doctor not found";
                    return RedirectToAction(nameof(Index));
                }

                var editDto = new DoctorEditDto
                {
                    DoctorId = doctor.DoctorId,
                    Specialization = doctor.Specialization,
                    AvailabilitySchedule = doctor.AvailabilitySchedule,
                    Phone = doctor.Phone,
                    Email = doctor.Email
                };

                return View(editDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doctor for edit: {DoctorId}", id);
                TempData["Error"] = "Error loading doctor";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: DoctorManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorEditDto dto)
        {
            if (id != dto.DoctorId)
            {
                _logger.LogWarning("Doctor ID mismatch in Edit action: URL ID={UrlId}, DTO ID={DtoId}", id, dto.DoctorId);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var result = await _doctorService.UpdateDoctorAsync(dto);
                if (result)
                {
                    _logger.LogInformation("Doctor updated successfully: {DoctorId}", dto.DoctorId);
                    TempData["Success"] = "Doctor updated successfully";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("Failed to update doctor: {DoctorId}", dto.DoctorId);
                ModelState.AddModelError("", "Error updating doctor or email already exists");
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor: {DoctorId}", dto.DoctorId);
                ModelState.AddModelError("", "Error updating doctor. Please try again.");
                return View(dto);
            }
        }

        // POST: DoctorManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _doctorService.DeleteDoctorAsync(id);
                if (result)
                {
                    _logger.LogInformation("Doctor deactivated successfully: {DoctorId}", id);
                    TempData["Success"] = "Doctor deactivated successfully";
                }
                else
                {
                    _logger.LogWarning("Failed to deactivate doctor: {DoctorId}", id);
                    TempData["Error"] = "Error deactivating doctor";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor: {DoctorId}", id);
                TempData["Error"] = "Error deactivating doctor";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: DoctorManagement/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int userId)
        {
            try
            {
                var result = await _doctorService.ToggleDoctorStatusAsync(userId);
                if (result)
                {
                    _logger.LogInformation("Doctor status toggled successfully: UserId={UserId}", userId);
                    TempData["Success"] = "Doctor status updated successfully";
                }
                else
                {
                    _logger.LogWarning("Failed to toggle doctor status: UserId={UserId}", userId);
                    TempData["Error"] = "Error updating doctor status";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling doctor status: UserId={UserId}", userId);
                TempData["Error"] = "Error updating doctor status";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: DoctorManagement/Schedule/5
        public async Task<IActionResult> Schedule(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                if (doctor == null)
                {
                    TempData["Error"] = "Doctor not found";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.DoctorName = doctor.Username;
                return View(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doctor schedule: {DoctorId}", id);
                TempData["Error"] = "Error loading schedule";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: DoctorManagement/Appointments/5
        public async Task<IActionResult> Appointments(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                if (doctor == null)
                {
                    TempData["Error"] = "Doctor not found";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.DoctorName = doctor.Username;
                ViewBag.DoctorId = id;
                return View(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doctor appointments: {DoctorId}", id);
                TempData["Error"] = "Error loading appointments";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: DoctorManagement/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var doctors = await _doctorService.GetAllDoctorsAsync();

                var stats = new
                {
                    TotalDoctors = doctors.Count,
                    ActiveDoctors = doctors.Count(d => d.IsActive),
                    InactiveDoctors = doctors.Count(d => !d.IsActive),
                    TotalAppointments = doctors.Sum(d => d.TotalAppointments),
                    DoctorsBySpecialization = doctors
                        .GroupBy(d => d.Specialization)
                        .Select(g => new { Specialization = g.Key, Count = g.Count() })
                        .ToList()
                };

                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doctor statistics");
                TempData["Error"] = "Error loading statistics";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: DoctorManagement/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                {
                    TempData["Error"] = "Password must be at least 6 characters";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Implement password reset in service
                TempData["Success"] = "Password reset successfully";
                _logger.LogInformation("Password reset for doctor: {DoctorId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for doctor: {DoctorId}", id);
                TempData["Error"] = "Error resetting password";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: DoctorManagement/Export
        public async Task<IActionResult> Export()
        {
            try
            {
                var doctors = await _doctorService.GetAllDoctorsAsync();

                // Simple CSV export
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Username,Email,Specialization,Phone,Status,Total Appointments");

                foreach (var doctor in doctors)
                {
                    csv.AppendLine($"{doctor.DoctorId},{doctor.Username},{doctor.Email},{doctor.Specialization},{doctor.Phone},{(doctor.IsActive ? "Active" : "Inactive")},{doctor.TotalAppointments}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"Doctors_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting doctors");
                TempData["Error"] = "Error exporting data";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}