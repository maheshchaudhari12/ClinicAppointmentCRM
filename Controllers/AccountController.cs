using ClinicAppointmentCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ClinicAppointmentCRM.Models;

namespace ClinicAppointmentCRM.Controllers
{
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : BaseMvcController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAuthService _authService;

        public AccountController(ILogger<AccountController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        /// <summary>
        /// Login Page - Public access
        /// </summary>
        [HttpGet("Login")]
        [AllowAnonymous] //  Anyone can access
        public IActionResult Login(string? returnUrl)
        {
            // If already authenticated, redirect to appropriate dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                return RedirectToDashboard(role);
            }

            ViewData["ReturnUrl"] = returnUrl;
            _logger.LogInformation("Login page accessed. ReturnUrl: {ReturnUrl}", returnUrl);
            return View();
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto loginDto, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(loginDto);

            var result = await _authService.Login(loginDto);
            var role = result.Role;

            if (result.Success)
            {
                // Option 1: Store JWT in HTTP-Only Cookie (RECOMMENDED)
                Response.Cookies.Append("jwt", result.Token!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Use HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = result.Expiration
                });

                // Option 2: Also store user info in Session (for UI display)
                HttpContext.Session.SetString("Username", result.Username!);
                HttpContext.Session.SetString("Role", result.Role!);
                HttpContext.Session.SetInt32("UserId", result.UserId);

                // Option 3: Store in TempData for immediate redirect
                TempData["SuccessMessage"] = result.Message;

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToDashboard(role);
            }

            ModelState.AddModelError(string.Empty, result.Message!);
            return View(loginDto);
        }
        /// <summary>
        /// Access Denied Page - Public (shows after unauthorized attempt)
        /// </summary>
        [HttpGet("AccessDenied")]
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            _logger.LogWarning("Access denied. User: {User}, ReturnUrl: {ReturnUrl}",
                User.Identity?.Name ?? "Anonymous", returnUrl);
            return View();
        }

        /// <summary>
        /// Logout - Requires authentication
        /// </summary>
        [HttpGet("Logout")]
        [HttpPost("Logout")]
        [Authorize] //  Must be logged in to logout
        public IActionResult Logout(string? returnUrl = null)
        {
            var username = User.Identity?.Name;
            _logger.LogInformation("User {Username} logged out", username);

            // Token removal happens on client-side
            // This just provides a server-side logout endpoint

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            Response.Cookies.Delete("jwt");
            HttpContext.Session.Clear();
            
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Change Password - Requires authentication
        /// </summary>
        [HttpGet("ChangePassword")]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Profile Settings - Requires authentication
        /// </summary>
        [HttpGet("Settings")]
        [Authorize]
        public IActionResult Settings()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            ViewBag.UserRole = role;
            return View();
        }

        public IActionResult ForgotPassword()
        {
            throw new NotImplementedException();
        }
    }
}