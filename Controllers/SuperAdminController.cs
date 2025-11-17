using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ClinicAppointmentCRM.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "SuperAdmin")] // ✅ Only Admin role can access
    [ApiExplorerSettings(IgnoreApi = true)] // ✅ Exclude from Swagger
    public class SuperAdminController : BaseMvcController
    {
        private readonly ILogger<SuperAdminController> _logger;
        private readonly ClinicDbContext _context;

        public SuperAdminController(ILogger<SuperAdminController> logger, ClinicDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// SuperAdmin Dashboard - Only accessible by Admins
        /// </summary>
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var adminId = GetCurrentAdminId();
            var isSuperAdmin = IsSuperAdmin();

            ViewBag.AdminId = adminId;
            ViewBag.IsSuperAdmin = isSuperAdmin;

            _logger.LogInformation("SuperAdmin Index page accessed by Admin ID: {AdminId}", adminId);
            return View();
        }

        /// <summary>
        /// Register new Admin - Only Super Admins can create other admins
        /// </summary>
        [HttpGet("RegisterAdmin")]
        [Authorize(Policy = "AdminOnly")] // Can also use policy
        public IActionResult RegisterAdmin()
        {
            if (!IsSuperAdmin())
            {
                _logger.LogWarning("Non-super admin attempted to access RegisterAdmin");
                return RedirectToAction("AccessDenied", "Account");
            }

            _logger.LogInformation("RegisterAdmin page accessed.");
            return View();
        }

        /// <summary>
        /// Process Admin Registration
        /// </summary>
        [HttpPost("RegisterAdmin")]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterAdmin(AdminRegDto adminRegDto)
        {
            if (!IsSuperAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin registration failed due to validation errors.");
                return View(adminRegDto);
            }

            try
            {
                // Create UserLogin
                var userLogin = new UserLogin
                {
                    Username = adminRegDto.Username,
                    Email = adminRegDto.Email,
                    Role = nameof(UserRole.Admin),
                    IsActive = adminRegDto.IsActive
                };

                if (adminRegDto.Password != null)
                    userLogin.PasswordHash = new PasswordHasher<UserLogin>()
                        .HashPassword(userLogin, adminRegDto.Password);

                _context.UserLogins.Add(userLogin);
                _context.SaveChanges();

                // Create Admin
                var admin = new Admin
                {
                    UserId = userLogin.UserId,
                    FullName = adminRegDto.FullName,
                    Phone = adminRegDto.Phone,
                    Email = adminRegDto.Email,
                    IsActive = adminRegDto.IsActive
                };

                _context.Admins.Add(admin);
                _context.SaveChanges();

                // Log Activation
                var currentAdminId = GetCurrentAdminId();
                var activationLog = new AdminActivationLog
                {
                    ActivatedAdminId = admin.AdminId,
                    ActivatedByAdminId = currentAdminId,
                    ActivationDate = DateTime.UtcNow
                };

                _context.AdminActivationLogs.Add(activationLog);
                _context.SaveChanges();

                _logger.LogInformation("Admin registration successful for {Username} by Admin ID: {AdminId}",
                    adminRegDto.Username, currentAdminId);

                TempData["SuccessMessage"] = $"Admin '{adminRegDto.FullName}' registered successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin registration for {Username}.", adminRegDto.Username);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(adminRegDto);
            }
        }

        /// <summary>
        /// Manage all admins - Super Admin only
        /// </summary>
        [HttpGet("ManageAdmins")]
        public IActionResult ManageAdmins()
        {
            if (!IsSuperAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var admins = _context.Admins
                .Select(a => new
                {
                    a.AdminId,
                    a.FullName,
                    a.Phone,
                    a.IsActive,
                    a.IsSuperAdmin,
                    a.UserLogin.Email,
                    a.UserLogin.Username
                })
                .ToList();

            return View(admins);
        }

        /// <summary>
        /// View all users
        /// </summary>
        [HttpGet("ManageUsers")]
        public IActionResult ManageUsers()
        {
            var users = _context.UserLogins
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt,
                    u.LastLogin
                })
                .ToList();

            return View(users);
        }

        /// <summary>
        /// Helper method to get current admin ID
        /// </summary>
        private int GetCurrentAdminId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var admin = _context.Admins.FirstOrDefault(a => a.UserId == userId);
                return admin?.AdminId ?? 0;
            }
            return 0;
        }

        /// <summary>
        /// Check if current user is super admin
        /// </summary>
        private bool IsSuperAdmin()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var admin = _context.Admins.FirstOrDefault(a => a.UserId == userId);
                return admin?.IsSuperAdmin ?? false;
            }
            return false;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}