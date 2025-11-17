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
    public class PatientManagementController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientManagementController> _logger;

        public PatientManagementController(IPatientService patientService, ILogger<PatientManagementController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        // GET: PatientManagement
        public async Task<IActionResult> Index(string searchTerm)
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patients");
                TempData["Error"] = "Error loading patients";
                return View(new List<PatientViewModel>());
            }
        }

        // GET: PatientManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                if (patient == null)
                {
                    TempData["Error"] = "Patient not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient details");
                TempData["Error"] = "Error loading patient details";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: PatientManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PatientManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientRegDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var result = await _patientService.CreatePatientAsync(dto);
                if (result)
                {
                    TempData["Success"] = "Patient created successfully";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Username or Email already exists");
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                ModelState.AddModelError("", "Error creating patient");
                return View(dto);
            }
        }

        // GET: PatientManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                if (patient == null)
                {
                    TempData["Error"] = "Patient not found";
                    return RedirectToAction(nameof(Index));
                }

                var editDto = new PatientEditDto
                {
                    PatientId = patient.PatientId,
                    FullName = patient.FullName,
                    DOB = patient.DOB,
                    Gender = patient.Gender,
                    Phone = patient.Phone,
                    Address = patient.Address,
                    Email = patient.Email
                };

                return View(editDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient for edit");
                TempData["Error"] = "Error loading patient";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: PatientManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientEditDto dto)
        {
            if (id != dto.PatientId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var result = await _patientService.UpdatePatientAsync(dto);
                if (result)
                {
                    TempData["Success"] = "Patient updated successfully";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Error updating patient");
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient");
                ModelState.AddModelError("", "Error updating patient");
                return View(dto);
            }
        }

        // POST: PatientManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _patientService.DeletePatientAsync(id);
                if (result)
                {
                    TempData["Success"] = "Patient deactivated successfully";
                }
                else
                {
                    TempData["Error"] = "Error deactivating patient";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient");
                TempData["Error"] = "Error deactivating patient";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: PatientManagement/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int userId)
        {
            try
            {
                var result = await _patientService.TogglePatientStatusAsync(userId);
                if (result)
                {
                    TempData["Success"] = "Patient status updated successfully";
                }
                else
                {
                    TempData["Error"] = "Error updating patient status";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling patient status");
                TempData["Error"] = "Error updating patient status";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
