using ClinicAppointmentCRM.Models;
using ClinicAppointmentCRM.Models.ViewModels;
using ClinicAppointmentCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAppointmentCRM.Controllers
{
    [Controller]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReceptionManagementController : Controller
    {
        private readonly IReceptionService _receptionService;
        private readonly ILogger<ReceptionManagementController> _logger;

        public ReceptionManagementController(
            IReceptionService receptionService,
            ILogger<ReceptionManagementController> logger)
        {
            _receptionService = receptionService;
            _logger = logger;
        }

        // GET: ReceptionManagement
        public async Task<IActionResult> Index(string searchTerm)
        {
            try
            {
                var receptions = await _receptionService.GetAllReceptionsAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View(receptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading receptionists");
                TempData["Error"] = "Error loading receptionists";
                return View(new List<ReceptionViewModel>());
            }
        }

        // GET: ReceptionManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var reception = await _receptionService.GetReceptionByIdAsync(id);
                if (reception == null)
                {
                    TempData["Error"] = "Receptionist not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(reception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading receptionist details for ID: {ReceptionId}", id);
                TempData["Error"] = "Error loading receptionist details";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: ReceptionManagement/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ReceptionManagement/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceptionRegDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var result = await _receptionService.CreateReceptionAsync(dto);
                if (result)
                {
                    _logger.LogInformation("Receptionist created successfully: {Username}", dto.Username);
                    TempData["Success"] = "Receptionist created successfully";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("Failed to create receptionist - Username or Email already exists: {Username}", dto.Username);
                ModelState.AddModelError("", "Username or Email already exists");
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating receptionist");
                ModelState.AddModelError("", "Error creating receptionist. Please try again.");
                return View(dto);
            }
        }

        // GET: ReceptionManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var reception = await _receptionService.GetReceptionByIdAsync(id);
                if (reception == null)
                {
                    TempData["Error"] = "Receptionist not found";
                    return RedirectToAction(nameof(Index));
                }

                var editDto = new ReceptionEditDto
                {
                    ReceptionId = reception.ReceptionId,
                    FullName = reception.FullName,
                    Phone = reception.Phone,
                    Email = reception.Email
                };

                return View(editDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading receptionist for edit: {ReceptionId}", id);
                TempData["Error"] = "Error loading receptionist";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ReceptionManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReceptionEditDto dto)
        {
            if (id != dto.ReceptionId)
            {
                _logger.LogWarning("Reception ID mismatch in Edit action: URL ID={UrlId}, DTO ID={DtoId}", id, dto.ReceptionId);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var result = await _receptionService.UpdateReceptionAsync(dto);
                if (result)
                {
                    _logger.LogInformation("Receptionist updated successfully: {ReceptionId}", dto.ReceptionId);
                    TempData["Success"] = "Receptionist updated successfully";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("Failed to update receptionist: {ReceptionId}", dto.ReceptionId);
                ModelState.AddModelError("", "Error updating receptionist or email already exists");
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating receptionist: {ReceptionId}", dto.ReceptionId);
                ModelState.AddModelError("", "Error updating receptionist. Please try again.");
                return View(dto);
            }
        }

        // POST: ReceptionManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _receptionService.DeleteReceptionAsync(id);
                if (result)
                {
                    _logger.LogInformation("Receptionist deactivated successfully: {ReceptionId}", id);
                    TempData["Success"] = "Receptionist deactivated successfully";
                }
                else
                {
                    _logger.LogWarning("Failed to deactivate receptionist: {ReceptionId}", id);
                    TempData["Error"] = "Error deactivating receptionist";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting receptionist: {ReceptionId}", id);
                TempData["Error"] = "Error deactivating receptionist";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ReceptionManagement/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int userId)
        {
            try
            {
                var result = await _receptionService.ToggleReceptionStatusAsync(userId);
                if (result)
                {
                    _logger.LogInformation("Receptionist status toggled successfully: UserId={UserId}", userId);
                    TempData["Success"] = "Receptionist status updated successfully";
                }
                else
                {
                    _logger.LogWarning("Failed to toggle receptionist status: UserId={UserId}", userId);
                    TempData["Error"] = "Error updating receptionist status";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling receptionist status: UserId={UserId}", userId);
                TempData["Error"] = "Error updating receptionist status";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ReceptionManagement/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var stats = await _receptionService.GetReceptionStatisticsAsync();
                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading receptionist statistics");
                TempData["Error"] = "Error loading statistics";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ReceptionManagement/ResetPassword/5
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

                var result = await _receptionService.ResetPasswordAsync(id, newPassword);
                if (result)
                {
                    _logger.LogInformation("Password reset for receptionist: {ReceptionId}", id);
                    TempData["Success"] = "Password reset successfully";
                }
                else
                {
                    TempData["Error"] = "Error resetting password";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for receptionist: {ReceptionId}", id);
                TempData["Error"] = "Error resetting password";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: ReceptionManagement/Performance/5
        public async Task<IActionResult> Performance(int id)
        {
            try
            {
                var reception = await _receptionService.GetReceptionByIdAsync(id);
                if (reception == null)
                {
                    TempData["Error"] = "Receptionist not found";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.ReceptionistName = reception.FullName;
                return View(reception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading receptionist performance: {ReceptionId}", id);
                TempData["Error"] = "Error loading performance data";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: ReceptionManagement/Export
        public async Task<IActionResult> Export()
        {
            try
            {
                var receptions = await _receptionService.GetAllReceptionsAsync();

                // Simple CSV export
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Username,Full Name,Email,Phone,Status,Appointments Handled");

                foreach (var reception in receptions)
                {
                    csv.AppendLine($"{reception.ReceptionId},{reception.Username},{reception.FullName},{reception.Email},{reception.Phone},{(reception.IsActive ? "Active" : "Inactive")},{reception.TotalAppointmentsHandled}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"Receptionists_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting receptionists");
                TempData["Error"] = "Error exporting data";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}