using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Models;
using ClinicAppointmentCRM.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentCRM.Services
{
    public class PatientService : IPatientService
    {
        private readonly ClinicDbContext _context;
        private readonly ILogger<PatientService> _logger;

        public PatientService(ClinicDbContext context, ILogger<PatientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PatientViewModel>> GetAllPatientsAsync(string searchTerm = null)
        {
            try
            {
                var query = _context.Patients
                    .Include(p => p.UserLogin)
                    .Include(p => p.Appointments)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p =>
                        p.FullName.Contains(searchTerm) ||
                        p.Phone.Contains(searchTerm) ||
                        p.UserLogin.Email.Contains(searchTerm) ||
                        p.UserLogin.Username.Contains(searchTerm));
                }

                var patients = await query
                    .Select(p => new PatientViewModel
                    {
                        PatientId = p.PatientId,
                        UserId = p.UserId,
                        Username = p.UserLogin.Username,
                        Email = p.UserLogin.Email,
                        FullName = p.FullName,
                        DOB = p.DOB,
                        Gender = p.Gender,
                        Phone = p.Phone,
                        Address = p.Address,
                        IsActive = p.UserLogin.IsActive,
                        LastLogin = p.UserLogin.LastLogin,
                        TotalAppointments = p.Appointments.Count
                    })
                    .OrderByDescending(p => p.PatientId)
                    .ToListAsync();

                return patients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patients");
                throw;
            }
        }

        public async Task<PatientViewModel> GetPatientByIdAsync(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.UserLogin)
                    .Include(p => p.Appointments)
                    .Where(p => p.PatientId == id)
                    .Select(p => new PatientViewModel
                    {
                        PatientId = p.PatientId,
                        UserId = p.UserId,
                        Username = p.UserLogin.Username,
                        Email = p.UserLogin.Email,
                        FullName = p.FullName,
                        DOB = p.DOB,
                        Gender = p.Gender,
                        Phone = p.Phone,
                        Address = p.Address,
                        IsActive = p.UserLogin.IsActive,
                        LastLogin = p.UserLogin.LastLogin,
                        TotalAppointments = p.Appointments.Count
                    })
                    .FirstOrDefaultAsync();

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patient by id: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreatePatientAsync(PatientRegDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if username or email exists
                if (await _context.UserLogins.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
                {
                    return false;
                }

                // Create UserLogin
                var passwordHasher = new PasswordHasher<UserLogin>();
                var userLogin = new UserLogin
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    Role = "Patient",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                userLogin.PasswordHash = passwordHasher.HashPassword(userLogin, dto.Password);

                _context.UserLogins.Add(userLogin);
                await _context.SaveChangesAsync();

                // Create Patient
                var patient = new Patient
                {
                    UserId = userLogin.UserId,
                    FullName = dto.FullName,
                    DOB = dto.Dob,
                    Gender = dto.Gender,
                    Phone = dto.Phone,
                    Address = dto.Address
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating patient");
                throw;
            }
        }

        public async Task<bool> UpdatePatientAsync(PatientEditDto dto)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.UserLogin)
                    .FirstOrDefaultAsync(p => p.PatientId == dto.PatientId);

                if (patient == null) return false;

                // Update Patient
                patient.FullName = dto.FullName;
                patient.DOB = dto.DOB;
                patient.Gender = dto.Gender;
                patient.Phone = dto.Phone;
                patient.Address = dto.Address;

                // Update Email in UserLogin
                patient.UserLogin.Email = dto.Email;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient");
                throw;
            }
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.UserLogin)
                    .FirstOrDefaultAsync(p => p.PatientId == id);

                if (patient == null) return false;

                // Soft delete - deactivate user
                patient.UserLogin.IsActive = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient");
                throw;
            }
        }

        public async Task<bool> TogglePatientStatusAsync(int userId)
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
                _logger.LogError(ex, "Error toggling patient status");
                throw;
            }
        }
    }
}
