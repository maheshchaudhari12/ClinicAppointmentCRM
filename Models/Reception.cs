using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicAppointmentCRM.Models
{
    public class Reception
    {
        [Key]
        public int ReceptionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        public virtual UserLogin UserLogin { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }


}