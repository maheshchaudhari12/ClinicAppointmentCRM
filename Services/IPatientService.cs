using ClinicAppointmentCRM.Models;
using ClinicAppointmentCRM.Models.ViewModels;

namespace ClinicAppointmentCRM.Services
{
    public interface IPatientService
    {
        Task<List<PatientViewModel>> GetAllPatientsAsync(string searchTerm = null);
        Task<PatientViewModel> GetPatientByIdAsync(int id);
        Task<bool> CreatePatientAsync(PatientRegDto dto);
        Task<bool> UpdatePatientAsync(PatientEditDto dto);
        Task<bool> DeletePatientAsync(int id);
        Task<bool> TogglePatientStatusAsync(int userId);
    }
}
