using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicAppointmentCRM.Configuration;
using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClinicAppointmentCRM.Services
{
    public class AuthService : IAuthService
    {
        private readonly ClinicDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ClinicDbContext context, IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public string GenerateJwtToken(UserLogin user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret ?? string.Empty));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResponseDto> Login(UserLoginDto loginDto)
        {
            try
            {
                var user = await _context.UserLogins
                    .FirstOrDefaultAsync(u =>
                        u.Username == loginDto.Username
                        && u.IsActive);

                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid credentials or account is inactive"
                    };
                }

                var passwordHasher = new PasswordHasher<UserLogin>();
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password ?? string.Empty);

                if (result == PasswordVerificationResult.Failed)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    };
                }

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    Role = user.Role,
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<AuthResponseDto> RegisterDoctor(DoctorRegDto doctorRegDto)
        {
            try
            {
                // Check if username or email already exists
                if (await _context.UserLogins.AnyAsync(u => u.Username == doctorRegDto.Username))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Username already exists"
                    };
                }

                if (await _context.UserLogins.AnyAsync(u => u.Email == doctorRegDto.Email))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email already exists"
                    };
                }

                // Create UserLogin
                var userLogin = new UserLogin
                {
                    Username = doctorRegDto.Username,
                    Email = doctorRegDto.Email,
                    Role = nameof(UserRole.Doctor),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var passwordHasher = new PasswordHasher<UserLogin>();
                userLogin.PasswordHash = passwordHasher.HashPassword(userLogin, doctorRegDto.Password ?? string.Empty);

                _context.UserLogins.Add(userLogin);
                await _context.SaveChangesAsync();

                // Create Doctor
                var doctor = new Doctor
                {
                    UserId = userLogin.UserId,
                    Specialization = doctorRegDto.Specialization ?? string.Empty,
                    AvailabilitySchedule = doctorRegDto.AvailabilitySchedule!,
                    Phone = doctorRegDto.Phone!
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(userLogin);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Doctor registered successfully",
                    Token = token,
                    Role = userLogin.Role,
                    UserId = userLogin.UserId,
                    Username = userLogin.Username,
                    Email = userLogin.Email,
                    Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during doctor registration");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        public async Task<AuthResponseDto> RegisterPatient(PatientRegDto patientRegDto)
        {
            try
            {
                if (await _context.UserLogins.AnyAsync(u => u.Username == patientRegDto.Username))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Username already exists"
                    };
                }

                if (await _context.UserLogins.AnyAsync(u => u.Email == patientRegDto.Email))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email already exists"
                    };
                }

                var userLogin = new UserLogin
                {
                    Username = patientRegDto.Username,
                    Email = patientRegDto.Email,
                    Role = nameof(UserRole.Patient),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var passwordHasher = new PasswordHasher<UserLogin>();
                userLogin.PasswordHash = passwordHasher.HashPassword(userLogin, patientRegDto.Password ?? string.Empty);

                _context.UserLogins.Add(userLogin);
                await _context.SaveChangesAsync();

                var patient = new Patient
                {
                    UserId = userLogin.UserId,
                    FullName = patientRegDto.FullName ?? string.Empty,
                    DOB = patientRegDto.Dob,
                    Gender = patientRegDto.Gender ?? string.Empty,
                    Phone = patientRegDto.Phone!,
                    Address = patientRegDto.Address!
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(userLogin);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Patient registered successfully",
                    Token = token,
                    Role = userLogin.Role,
                    UserId = userLogin.UserId,
                    Username = userLogin.Username,
                    Email = userLogin.Email,
                    Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during patient registration");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        public async Task<AuthResponseDto> RegisterReception(ReceptionRegDto receptionRegDto)
        {
            try
            {
                if (await _context.UserLogins.AnyAsync(u => u.Username == receptionRegDto.Username))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Username already exists"
                    };
                }

                if (await _context.UserLogins.AnyAsync(u => u.Email == receptionRegDto.Email))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email already exists"
                    };
                }

                var userLogin = new UserLogin
                {
                    Username = receptionRegDto.Username,
                    Email = receptionRegDto.Email,
                    Role = nameof(UserRole.Reception),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var passwordHasher = new PasswordHasher<UserLogin>();
                userLogin.PasswordHash = passwordHasher.HashPassword(userLogin, receptionRegDto.Password ?? string.Empty);

                _context.UserLogins.Add(userLogin);
                await _context.SaveChangesAsync();

                var reception = new Reception
                {
                    UserId = userLogin.UserId,
                    FullName = receptionRegDto.FullName ?? string.Empty,
                    Phone = receptionRegDto.Phone!
                };

                _context.Receptions.Add(reception);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(userLogin);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Receptionist registered successfully",
                    Token = token,
                    Role = userLogin.Role,
                    UserId = userLogin.UserId,
                    Username = userLogin.Username,
                    Email = userLogin.Email,
                    Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reception registration");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }
    }
}