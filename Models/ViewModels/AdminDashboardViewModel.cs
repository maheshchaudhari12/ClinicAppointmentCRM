namespace ClinicAppointmentCRM.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int TotalReceptionists { get; set; }
        public int ActiveReceptionists { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }

        public List<PatientViewModel> RecentPatients { get; set; }
        public List<DoctorViewModel> RecentDoctors { get; set; }
    }
}
