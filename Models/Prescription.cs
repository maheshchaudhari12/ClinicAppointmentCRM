using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentCRM.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [Display(Name = "Medication Details")]
        public string MedicationDetails { get; set; }

        [Required]
        [Display(Name = "Dosage")]
        public string Dosage { get; set; }

        [StringLength(500)]
        [Display(Name = "Instructions")]
        public string Instructions { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Issued Date")]
        public DateTime IssuedDate { get; set; }

        public virtual Appointment Appointment { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual Patient Patient { get; set; }
    }


}