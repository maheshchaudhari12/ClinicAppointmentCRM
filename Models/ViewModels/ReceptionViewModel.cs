using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentCRM.Models.ViewModels
{
    public class ReceptionViewModel
    {
        public int ReceptionId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public int TotalAppointmentsHandled { get; set; }
    }

    public class ReceptionEditDto
    {
        public int ReceptionId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
