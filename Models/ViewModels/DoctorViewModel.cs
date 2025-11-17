using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentCRM.Models.ViewModels
{
    public class DoctorViewModel
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Specialization { get; set; }
        public string AvailabilitySchedule { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public int TotalAppointments { get; set; }
    }

    public class DoctorEditDto
    {
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(100)]
        public string Specialization { get; set; }

        [StringLength(200)]
        public string AvailabilitySchedule { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
