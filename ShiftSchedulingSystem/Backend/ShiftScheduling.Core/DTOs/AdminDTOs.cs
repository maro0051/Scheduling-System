using System.ComponentModel.DataAnnotations;

namespace ShiftScheduling.Core.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? Password { get; set; } // Optional - will generate if not provided
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty; // "Manager" or "Employee"
        
        [Phone]
        public string? PhoneNumber { get; set; }
        
        public bool SendEmail { get; set; } = true; // Option to send email notification
    }

    public class UpdateUserDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        [Phone]
        public string? PhoneNumber { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}