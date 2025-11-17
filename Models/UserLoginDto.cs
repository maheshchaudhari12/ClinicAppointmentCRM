using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentCRM.Models
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Invalid Username")]
        [RegularExpression("^[a-zA-Z0-9_]+$",
           ErrorMessage = "Invalid Username")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Invalid Password")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&]).{6,}$",
           ErrorMessage = "Invalid Password")]
        public string? Password { get; set; }

    }
}