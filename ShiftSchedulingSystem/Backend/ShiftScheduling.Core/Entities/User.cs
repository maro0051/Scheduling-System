using System.ComponentModel.DataAnnotations;

namespace ShiftScheduling.Core.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty; // "Manager" or "Employee"
        
        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties - these create relationships with other tables
        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
        public virtual ICollection<ShiftSwapRequest> SentSwapRequests { get; set; } = new List<ShiftSwapRequest>();
        public virtual ICollection<ShiftSwapRequest> ReceivedSwapRequests { get; set; } = new List<ShiftSwapRequest>();
    }
}