using ClinicAppointmentCRM.Models;
using ClinicAppointmentCRM.Models.ViewModels;

namespace ClinicAppointmentCRM.Services
{
    public interface IDoctorService
    {
        Task<List<DoctorViewModel>> GetAllDoctorsAsync(string searchTerm = null);
        Task<DoctorViewModel> GetDoctorByIdAsync(int id);
        Task<bool> CreateDoctorAsync(DoctorRegDto dto);
        Task<bool> UpdateDoctorAsync(DoctorEditDto dto);
        Task<bool> DeleteDoctorAsync(int id);
        Task<bool> ToggleDoctorStatusAsync(int userId);
    }
}
