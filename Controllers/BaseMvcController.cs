using Microsoft.AspNetCore.Mvc;

namespace ClinicAppointmentCRM.Controllers
{
    /// <summary>
    /// Base controller for all MVC controllers (Views)
    /// Excluded from Swagger documentation
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class BaseMvcController : Controller
    {
        // Common functionality for MVC controllers can go here

        protected IActionResult RedirectToDashboard(string? role)
        {
            return role switch
            {
                "SuperAdmin" => RedirectToAction("Index", "SuperAdmin"),
                "Admin" => RedirectToAction("Index", "Admin"),
                "Doctor" => RedirectToAction("Dashboard", "Doctor"),
                "Patient" => RedirectToAction("Dashboard", "Patient"),
                "Reception" => RedirectToAction("Dashboard", "Reception"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}