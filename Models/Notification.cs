using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicAppointmentCRM.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Message Body")]
        public string Body { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Sent On")]
        public DateTime SentDateTime { get; set; }

        [Display(Name = "Delivery Status")]
        public string Status { get; set; }

        public virtual UserLogin UserLogin { get; set; }
    }


}