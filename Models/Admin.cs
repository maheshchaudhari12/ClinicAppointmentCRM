using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentCRM.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        public string? Email { get; set; }


        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Is Super Admin")]
        public bool IsSuperAdmin { get; set; }

        public virtual UserLogin UserLogin { get; set; }
        public virtual ICollection<AdminActivationLog> ActivatedAdmins { get; set; }
        public virtual ICollection<AdminActivationLog> ActivatedBy { get; set; }
    }


}