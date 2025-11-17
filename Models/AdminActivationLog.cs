using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentCRM.Models
{
    public class AdminActivationLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public int ActivatedAdminId { get; set; }

        [Required]
        public int ActivatedByAdminId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Activation Date")]
        public DateTime ActivationDate { get; set; }

        public virtual Admin ActivatedAdmin { get; set; }
        public virtual Admin ActivatedBy { get; set; }
    }

}