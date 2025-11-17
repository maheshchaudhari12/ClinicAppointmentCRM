using ClinicAppointmentCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ClinicAppointmentCRM.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous] // ✅ Entire controller is public
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Home page - Public
        /// </summary>
        public IActionResult Index()
        {
            // Check if user is authenticated
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            if (ViewBag.IsAuthenticated)
            {
                ViewBag.Username = User.Identity?.Name;
                ViewBag.Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            }

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}