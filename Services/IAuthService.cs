using ClinicAppointmentCRM.Models;

namespace ClinicAppointmentCRM.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(UserLogin user);
        Task<AuthResponseDto> Login(UserLoginDto loginDto);
        Task<AuthResponseDto> RegisterDoctor(DoctorRegDto doctorRegDto);
        Task<AuthResponseDto> RegisterPatient(PatientRegDto patientRegDto);
        Task<AuthResponseDto> RegisterReception(ReceptionRegDto receptionRegDto);
    }
}