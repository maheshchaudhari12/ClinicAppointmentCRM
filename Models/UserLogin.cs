using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicAppointmentCRM.Models
{
    public class UserLogin
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; }

        [Required]
        [EnumDataType(typeof(UserRole))]
        [Display(Name = "User Role")]
        public string Role { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public virtual Admin Admin { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Reception Reception { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }

    public enum UserRole
    {
        Admin,
        Doctor,
        Patient,
        Reception
    }


}