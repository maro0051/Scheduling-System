using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftScheduling.API.Services;
using ShiftScheduling.Core.DTOs;
using ShiftScheduling.Core.Entities;
using ShiftScheduling.Infrastructure.Repositories;

namespace ShiftScheduling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AdminController(
            IRepository<User> userRepository, 
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
        }

        // Create a new user (Manager or Employee)
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            var exists = await _userRepository.ExistsAsync(u => u.Email == createUserDto.Email);
            if (exists)
                return BadRequest(new { message = "User with this email already exists" });

            // Generate random password if not provided
            var password = createUserDto.Password ?? GenerateRandomPassword();
            
            var user = new User
            {
                Email = createUserDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Role = createUserDto.Role,
                PhoneNumber = createUserDto.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // Send welcome email with credentials
            try
            {
                await _emailService.SendWelcomeEmailAsync(
                    user.Email, 
                    $"{user.FirstName} {user.LastName}", 
                    user.Role, 
                    password
                );
            }
            catch (Exception ex)
            {
                // Log error but don't fail the user creation
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }

            return Ok(new
            {
                message = $"{createUserDto.Role} created successfully. Login credentials sent to {user.Email}",
                user = new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.Role,
                    temporaryPassword = password
                }
            });
        }

        // Get all users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var userList = users.Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.PhoneNumber,
                u.IsActive,
                u.CreatedAt
            });
            return Ok(userList);
        }

        // Get user by ID
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role,
                user.PhoneNumber,
                user.IsActive,
                user.CreatedAt
            });
        }

        // Update user
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.Role = updateUserDto.Role;
            user.PhoneNumber = updateUserDto.PhoneNumber;
            user.IsActive = updateUserDto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return Ok(new { message = "User updated successfully" });
        }

        // Reset user password
        [HttpPost("users/{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var newPassword = GenerateRandomPassword();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);

            // Send email with new password
            try
            {
                await _emailService.SendPasswordResetEmailAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    newPassword
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send password reset email: {ex.Message}");
            }

            return Ok(new
            {
                message = $"Password reset successfully. New password sent to {user.Email}",
                temporaryPassword = newPassword
            });
        }

        // Delete user
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Prevent deleting the last admin
            var admins = await _userRepository.FindAsync(u => u.Role == "Admin");
            if (user.Role == "Admin" && admins.Count() <= 1)
                return BadRequest(new { message = "Cannot delete the last admin user" });

            await _userRepository.DeleteAsync(user);
            return Ok(new { message = "User deleted successfully" });
        }

        // Get statistics
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var allUsers = await _userRepository.GetAllAsync();
            
            var stats = new
            {
                totalUsers = allUsers.Count(),
                totalAdmins = allUsers.Count(u => u.Role == "Admin"),
                totalManagers = allUsers.Count(u => u.Role == "Manager"),
                totalEmployees = allUsers.Count(u => u.Role == "Employee"),
                activeUsers = allUsers.Count(u => u.IsActive),
                inactiveUsers = allUsers.Count(u => !u.IsActive)
            };
            
            return Ok(stats);
        }

        // Resend welcome email to user
        [HttpPost("users/{id}/resend-welcome-email")]
        public async Task<IActionResult> ResendWelcomeEmail(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Generate a new temporary password
            var newPassword = GenerateRandomPassword();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);

            // Send welcome email
            try
            {
                await _emailService.SendWelcomeEmailAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    user.Role,
                    newPassword
                );
                
                return Ok(new
                {
                    message = $"Welcome email resent to {user.Email} with new temporary password",
                    temporaryPassword = newPassword
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to send email: {ex.Message}" });
            }
        }

        private string GenerateRandomPassword()
        {
            // Generate a random 10-character password for better security
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return password;
        }
    }
}