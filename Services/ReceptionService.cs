using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Models;
using ClinicAppointmentCRM.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentCRM.Services
{
    public class ReceptionService : IReceptionService
    {
        private readonly ClinicDbContext _context;
        private readonly ILogger<ReceptionService> _logger;

        public ReceptionService(ClinicDbContext context, ILogger<ReceptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ReceptionViewModel>> GetAllReceptionsAsync(string searchTerm = null)
        {
            try
            {
                var query = _context.Receptions
                    .Include(r => r.UserLogin)
                    .Include(r => r.Appointments)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(r =>
                        r.FullName.Contains(searchTerm) ||
                        r.Phone.Contains(searchTerm) ||
                        r.UserLogin.Email.Contains(searchTerm) ||
                        r.UserLogin.Username.Contains(searchTerm));
                }

                var receptions = await query
                    .Select(r => new ReceptionViewModel
                    {
                        ReceptionId = r.ReceptionId,
                        UserId = r.UserId,
                        Username = r.UserLogin.Username,
                        Email = r.UserLogin.Email,
                        FullName = r.FullName,
                        Phone = r.Phone,
                        IsActive = r.UserLogin.IsActive,
                        LastLogin = r.UserLogin.LastLogin,
                        TotalAppointmentsHandled = r.Appointments.Count
                    })
                    .OrderByDescending(r => r.ReceptionId)
                    .ToListAsync();

                return receptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching receptionists");
                throw;
            }
        }

        public async Task<ReceptionViewModel> GetReceptionByIdAsync(int id)
        {
            try
            {
                var reception = await _context.Receptions
                    .Include(r => r.UserLogin)
                    .Include(r => r.Appointments)
                    .Where(r => r.ReceptionId == id)
                    .Select(r => new ReceptionViewModel
                    {
                        ReceptionId = r.ReceptionId,
                        UserId = r.UserId,
                        Username = r.UserLogin.Username,
                        Email = r.UserLogin.Email,
                        FullName = r.FullName,
                        Phone = r.Phone,
                        IsActive = r.UserLogin.IsActive,
                        LastLogin = r.UserLogin.LastLogin,
                        TotalAppointmentsHandled = r.Appointments.Count
                    })
                    .FirstOrDefaultAsync();

                return reception;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching receptionist by id: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateReceptionAsync(ReceptionRegDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if username or email exists
                var existingUser = await _context.UserLogins
                    .AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email);

                if (existingUser)
                {
                    _logger.LogWarning("Username or email already exists: {Username}, {Email}", dto.Username, dto.Email);
                    return false;
                }

                // Create UserLogin
                var passwordHasher = new PasswordHasher<UserLogin>();
                var userLogin = new UserLogin
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    Role = "Reception",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                userLogin.PasswordHash = passwordHasher.HashPassword(userLogin, dto.Password ?? string.Empty);

                _context.UserLogins.Add(userLogin);
                await _context.SaveChangesAsync();

                // Create Reception
                var reception = new Reception
                {
                    UserId = userLogin.UserId,
                    FullName = dto.FullName,
                    Phone = dto.Phone
                };

                _context.Receptions.Add(reception);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Reception created successfully: {Username}", dto.Username);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating receptionist");
                throw;
            }
        }

        public async Task<bool> UpdateReceptionAsync(ReceptionEditDto dto)
        {
            try
            {
                var reception = await _context.Receptions
                    .Include(r => r.UserLogin)
                    .FirstOrDefaultAsync(r => r.ReceptionId == dto.ReceptionId);

                if (reception == null)
                {
                    _logger.LogWarning("Reception not found for update: {ReceptionId}", dto.ReceptionId);
                    return false;
                }

                // Check if email is being changed to an existing one
                var emailExists = await _context.UserLogins
                    .AnyAsync(u => u.Email == dto.Email && u.UserId != reception.UserId);

                if (emailExists)
                {
                    _logger.LogWarning("Email already exists: {Email}", dto.Email);
                    return false;
                }

                // Update Reception
                reception.FullName = dto.FullName;
                reception.Phone = dto.Phone;

                // Update Email in UserLogin
                reception.UserLogin.Email = dto.Email;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Reception updated successfully: {ReceptionId}", dto.ReceptionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating receptionist");
                throw;
            }
        }

        public async Task<bool> DeleteReceptionAsync(int id)
        {
            try
            {
                var reception = await _context.Receptions
                    .Include(r => r.UserLogin)
                    .Include(r => r.Appointments)
                    .FirstOrDefaultAsync(r => r.ReceptionId == id);

                if (reception == null)
                {
                    _logger.LogWarning("Reception not found for deletion: {ReceptionId}", id);
                    return false;
                }

                // Check if receptionist has handled appointments
                if (reception.Appointments != null && reception.Appointments.Any())
                {
                    // Soft delete - deactivate user instead of hard delete
                    reception.UserLogin.IsActive = false;
                    _logger.LogInformation("Reception deactivated (has appointments): {ReceptionId}", id);
                }
                else
                {
                    // Can safely deactivate
                    reception.UserLogin.IsActive = false;
                    _logger.LogInformation("Reception deactivated: {ReceptionId}", id);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting receptionist");
                throw;
            }
        }

        public async Task<bool> ToggleReceptionStatusAsync(int userId)
        {
            try
            {
                var user = await _context.UserLogins.FindAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found for status toggle: {UserId}", userId);
                    return false;
                }

                if (user.Role != "Reception")
                {
                    _logger.LogWarning("User is not a receptionist: {UserId}, Role: {Role}", userId, user.Role);
                    return false;
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reception status toggled: {UserId}, New Status: {IsActive}", userId, user.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling receptionist status");
                throw;
            }
        }

        // Additional helper methods

        public async Task<int> GetActiveReceptionCountAsync()
        {
            try
            {
                return await _context.Receptions
                    .CountAsync(r => r.UserLogin.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active reception count");
                throw;
            }
        }

        public async Task<List<ReceptionViewModel>> GetActiveReceptionsAsync()
        {
            try
            {
                var receptions = await _context.Receptions
                    .Include(r => r.UserLogin)
                    .Include(r => r.Appointments)
                    .Where(r => r.UserLogin.IsActive)
                    .Select(r => new ReceptionViewModel
                    {
                        ReceptionId = r.ReceptionId,
                        UserId = r.UserId,
                        Username = r.UserLogin.Username,
                        Email = r.UserLogin.Email,
                        FullName = r.FullName,
                        Phone = r.Phone,
                        IsActive = r.UserLogin.IsActive,
                        LastLogin = r.UserLogin.LastLogin,
                        TotalAppointmentsHandled = r.Appointments.Count
                    })
                    .OrderBy(r => r.FullName)
                    .ToListAsync();

                return receptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active receptionists");
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(int receptionId, string newPassword)
        {
            try
            {
                var reception = await _context.Receptions
                    .Include(r => r.UserLogin)
                    .FirstOrDefaultAsync(r => r.ReceptionId == receptionId);

                if (reception == null)
                {
                    _logger.LogWarning("Reception not found for password reset: {ReceptionId}", receptionId);
                    return false;
                }

                var passwordHasher = new PasswordHasher<UserLogin>();
                reception.UserLogin.PasswordHash = passwordHasher.HashPassword(reception.UserLogin, newPassword);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Password reset for reception: {ReceptionId}", receptionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for receptionist");
                throw;
            }
        }

        public async Task<ReceptionViewModel> GetReceptionByUserIdAsync(int userId)
        {
            try
            {
                var reception = await _context.Receptions
                    .Include(r => r.UserLogin)
                    .Include(r => r.Appointments)
                    .Where(r => r.UserId == userId)
                    .Select(r => new ReceptionViewModel
                    {
                        ReceptionId = r.ReceptionId,
                        UserId = r.UserId,
                        Username = r.UserLogin.Username,
                        Email = r.UserLogin.Email,
                        FullName = r.FullName,
                        Phone = r.Phone,
                        IsActive = r.UserLogin.IsActive,
                        LastLogin = r.UserLogin.LastLogin,
                        TotalAppointmentsHandled = r.Appointments.Count
                    })
                    .FirstOrDefaultAsync();

                return reception;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching receptionist by user id: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int receptionId)
        {
            try
            {
                return await _context.Receptions.AnyAsync(r => r.ReceptionId == receptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if receptionist exists");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetReceptionStatisticsAsync()
        {
            try
            {
                var stats = new Dictionary<string, int>
                {
                    ["Total"] = await _context.Receptions.CountAsync(),
                    ["Active"] = await _context.Receptions.CountAsync(r => r.UserLogin.IsActive),
                    ["Inactive"] = await _context.Receptions.CountAsync(r => !r.UserLogin.IsActive),
                    ["TotalAppointmentsHandled"] = await _context.Appointments
                        .Where(a => a.Reception != null)
                        .CountAsync()
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reception statistics");
                throw;
            }
        }
    }
}