// Services/IReceptionService.cs
using ClinicAppointmentCRM.Models;
using ClinicAppointmentCRM.Models.ViewModels;

namespace ClinicAppointmentCRM.Services
{
    public interface IReceptionService
    {
        // CRUD Operations
        Task<List<ReceptionViewModel>> GetAllReceptionsAsync(string searchTerm = null);
        Task<ReceptionViewModel> GetReceptionByIdAsync(int id);
        Task<bool> CreateReceptionAsync(ReceptionRegDto dto);
        Task<bool> UpdateReceptionAsync(ReceptionEditDto dto);
        Task<bool> DeleteReceptionAsync(int id);
        Task<bool> ToggleReceptionStatusAsync(int userId);

        // Additional Methods
        Task<int> GetActiveReceptionCountAsync();
        Task<List<ReceptionViewModel>> GetActiveReceptionsAsync();
        Task<bool> ResetPasswordAsync(int receptionId, string newPassword);
        Task<ReceptionViewModel> GetReceptionByUserIdAsync(int userId);
        Task<bool> ExistsAsync(int receptionId);
        Task<Dictionary<string, int>> GetReceptionStatisticsAsync();
    }
}