using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentCRM.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Specialization")]
        public string Specialization { get; set; }

        [StringLength(200)]
        [Display(Name = "Availability Schedule")]
        public string AvailabilitySchedule { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        public virtual UserLogin UserLogin { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; }
    }


}