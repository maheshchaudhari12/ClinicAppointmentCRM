using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Models;
using ClinicAppointmentCRM.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentCRM.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ClinicDbContext _context;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(ClinicDbContext context, ILogger<DoctorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<DoctorViewModel>> GetAllDoctorsAsync(string searchTerm = null)
        {
            try
            {
                var query = _context.Doctors
                    .Include(d => d.UserLogin)
                    .Include(d => d.Appointments)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(d =>
                        d.Specialization.Contains(searchTerm) ||
                        d.Phone.Contains(searchTerm) ||
                        d.UserLogin.Email.Contains(searchTerm) ||
                        d.UserLogin.Username.Contains(searchTerm));
                }

                var doctors = await query
                    .Select(d => new DoctorViewModel
                    {
                        DoctorId = d.DoctorId,
                        UserId = d.UserId,
                        Username = d.UserLogin.Username,
                        Email = d.UserLogin.Email,
                        Specialization = d.Specialization,
                        AvailabilitySchedule = d.AvailabilitySchedule,
                        Phone = d.Phone,
                        IsActive = d.UserLogin.IsActive,
                        LastLogin = d.UserLogin.LastLogin,
                        TotalAppointments = d.Appointments.Count
                    })
                    .OrderByDescending(d => d.DoctorId)
                    .ToListAsync();

                return doctors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors");
                throw;
            }
        }

        public async Task<DoctorViewModel> GetDoctorByIdAsync(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.UserLogin)
                .Include(d => d.Appointments)
                .Where(d => d.DoctorId == id)
                .Select(d => new DoctorViewModel
                {
                    DoctorId = d.DoctorId,
                    UserId = d.UserId,
                    Username = d.UserLogin.Username,
                    Email = d.UserLogin.Email,
                    Specialization = d.Specialization,
                    AvailabilitySchedule = d.AvailabilitySchedule,
                    Phone = d.Phone,
                    IsActive = d.UserLogin.IsActive,
                    LastLogin = d.UserLogin.LastLogin,
                    TotalAppointments = d.Appointments.Count
                })
                .FirstOrDefaultAsync();

            return doctor;
        }

        public async Task<bool> CreateDoctorAsync(DoctorRegDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (await _context.UserLogins.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
                {
                    return false;
                }

                var passwordHasher = new PasswordHasher<UserLogin>();
                var userLogin = new UserLogin
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    Role = "Doctor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                userLogin.PasswordHash = passwordHasher.HashPassword(userLogin, dto.Password);

                _context.UserLogins.Add(userLogin);
                await _context.SaveChangesAsync();

                var doctor = new Doctor
                {
                    UserId = userLogin.UserId,
                    Specialization = dto.Specialization,
                    AvailabilitySchedule = dto.AvailabilitySchedule,
                    Phone = dto.Phone
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating doctor");
                throw;
            }
        }

        public async Task<bool> UpdateDoctorAsync(DoctorEditDto dto)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.UserLogin)
                    .FirstOrDefaultAsync(d => d.DoctorId == dto.DoctorId);

                if (doctor == null) return false;

                doctor.Specialization = dto.Specialization;
                doctor.AvailabilitySchedule = dto.AvailabilitySchedule;
                doctor.Phone = dto.Phone;
                doctor.UserLogin.Email = dto.Email;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor");
                throw;
            }
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.UserLogin)
                    .FirstOrDefaultAsync(d => d.DoctorId == id);

                if (doctor == null) return false;

                doctor.UserLogin.IsActive = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor");
                throw;
            }
        }

        public async Task<bool> ToggleDoctorStatusAsync(int userId)
        {
            try
            {
                var user = await _context.UserLogins.FindAsync(userId);
                if (user == null) return false;

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling doctor status");
                throw;
            }
        }
    }
}
