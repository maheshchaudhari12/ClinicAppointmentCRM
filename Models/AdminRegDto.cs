using System.ComponentModel.DataAnnotations;


namespace ClinicAppointmentCRM.Models
{
    public class AdminRegDto
    {
        // Full Name: Required, max 100 characters, only letters and spaces
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name must contain only letters and spaces")]
        public string? FullName { get; set; }

        // Phone: Required, valid format, 10–15 digits
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Phone number must start with 6, 7, 8, or 9 and be 10 digits")]
        public string? Phone { get; set; }

        // Username: Required, alphanumeric, 4–50 characters
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 50 characters")]
        [RegularExpression("^[a-zA-Z0-9_]+$",
            ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public string? Username { get; set; }

        // Password: Required, strong password rule
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&]).{6,}$",
            ErrorMessage = "Password must include uppercase, lowercase, number, and special character")]
        public string? Password { get; set; }

        // Email: Required, valid format
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address format")]
        public string? Email { get; set; }

        // Flags whether the admin account is active
        public bool IsActive { get; set; } = true;
    }

}